using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ElegantSuits.Domain.Entities;
using ElegantSuits.Domain.Enums;
using ElegantSuits.Web.Models;
using ElegantSuits.Web.Services;

namespace ElegantSuits.Web.Controllers;

public class ShoppingCartController : Controller
{
    private readonly ICartApiClient _cartApiClient;
    private readonly IProductApiClient _productApiClient;
    private readonly ICouponApiClient _couponApiClient;
    private readonly IOrderApiClient _orderApiClient;
    private readonly ILogger<ShoppingCartController> _logger;

    public ShoppingCartController(
        ICartApiClient cartApiClient,
        IProductApiClient productApiClient,
        ICouponApiClient couponApiClient,
        IOrderApiClient orderApiClient,
        ILogger<ShoppingCartController> logger)
    {
        _cartApiClient = cartApiClient;
        _productApiClient = productApiClient;
        _couponApiClient = couponApiClient;
        _orderApiClient = orderApiClient;
        _logger = logger;
    }

    private string? GetToken() => HttpContext.Session.GetString("JwtToken");

    public async Task<IActionResult> Index()
    {
        var token = GetToken();
        var res = await _cartApiClient.GetCartAsync(token);
        var cart = res.Data ?? new CartViewModel();

        var cartItems = cart.Items.Select(i => new CartItem
        {
            Id = i.Id,
            ProductId = i.ProductId,
            Quantity = i.Quantity,
            Product = new Product
            {
                Id = i.ProductId,
                Name = i.ProductName,
                Price = i.Price,
                ImageUrl = i.ImageUrl
            }
        }).ToList();

        ViewBag.TotalPrice = cart.TotalPrice;
        ViewBag.Discount = cart.Discount;
        return View(cartItems);
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart(int productId, int quantity = 1, string? size = null)
    {
        var token = GetToken();
        var req = new AddToCartRequest
        {
            ProductId = productId,
            Quantity = quantity,
            Size = size
        };

        var res = await _cartApiClient.AddToCartAsync(req, token);
        if (res.IsSuccess)
        {
            TempData["SuccessMessage"] = "Đã thêm sản phẩm vào giỏ hàng!";
        }
        else
        {
            TempData["ErrorMessage"] = res.Message ?? "Lỗi thêm giỏ hàng";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> UpdateCart(int id, int quantity)
    {
        var token = GetToken();
        var res = await _cartApiClient.UpdateCartItemAsync(id, quantity, token);
        return Json(new { success = res.IsSuccess, message = res.Message });
    }

    [HttpPost]
    public async Task<IActionResult> RemoveFromCart(int id)
    {
        var token = GetToken();
        var res = await _cartApiClient.RemoveCartItemAsync(id, token);
        return Json(new { success = res.IsSuccess, message = res.Message });
    }

    [HttpPost]
    public async Task<IActionResult> ClearCart()
    {
        var token = GetToken();
        var res = await _cartApiClient.ClearCartAsync(token);
        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    public async Task<IActionResult> Checkout()
    {
        var token = GetToken();
        var res = await _cartApiClient.GetCartAsync(token);
        var cart = res.Data ?? new CartViewModel();

        if (!cart.Items.Any())
        {
            TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống!";
            return RedirectToAction(nameof(Index));
        }

        var order = new Order
        {
            TotalPrice = cart.TotalPrice,
            ShippingAddress = "",
            OrderDate = DateTime.Now
        };

        ViewBag.CartItems = cart.Items;
        return View(order);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(Order order, string paymentMethod = "COD")
    {
        var token = GetToken();
        var req = new CreateOrderRequest
        {
            CustomerName = User.Identity?.Name ?? "Khách hàng",
            PhoneNumber = "",
            ShippingAddress = order.ShippingAddress,
            Notes = order.Notes,
            PaymentMethod = paymentMethod
        };

        var res = await _orderApiClient.CreateOrderAsync(req, token);
        if (res.IsSuccess && res.Data > 0)
        {
            int orderId = res.Data;
            if (paymentMethod == "VnPay")
            {
                return RedirectToAction("CreateVnPayPayment", "Payment", new { orderId });
            }
            return RedirectToAction(nameof(OrderCompleted), new { id = orderId });
        }

        TempData["ErrorMessage"] = res.Message ?? "Lỗi khi tạo đơn hàng.";
        return View(order);
    }

    public IActionResult OrderCompleted(int id)
    {
        return View(id);
    }

    [Authorize]
    public async Task<IActionResult> MyOrders()
    {
        var token = GetToken();
        var res = await _orderApiClient.GetUserOrdersAsync(token);
        var orders = res.Data ?? new List<OrderViewModel>();

        var mapped = orders.Select(o => new Order
        {
            Id = o.Id,
            OrderDate = o.OrderDate,
            TotalPrice = o.TotalPrice,
            PaymentMethod = o.PaymentMethod,
            PaymentStatus = Enum.TryParse<PaymentStatus>(o.PaymentStatus, out var ps) ? ps : PaymentStatus.Pending,
            OrderStatus = Enum.TryParse<OrderStatus>(o.OrderStatus, out var os) ? os : OrderStatus.Pending,
            ShippingAddress = o.ShippingAddress,
            Notes = o.Notes
        }).ToList();

        return View(mapped);
    }

    [Authorize]
    public async Task<IActionResult> OrderDetails(int id)
    {
        var token = GetToken();
        var res = await _orderApiClient.GetOrderByIdAsync(id, token);
        if (!res.IsSuccess || res.Data == null) return NotFound();

        var o = res.Data;
        var order = new Order
        {
            Id = o.Id,
            OrderDate = o.OrderDate,
            TotalPrice = o.TotalPrice,
            PaymentMethod = o.PaymentMethod,
            PaymentStatus = Enum.TryParse<PaymentStatus>(o.PaymentStatus, out var ps) ? ps : PaymentStatus.Pending,
            OrderStatus = Enum.TryParse<OrderStatus>(o.OrderStatus, out var os) ? os : OrderStatus.Pending,
            ShippingAddress = o.ShippingAddress,
            Notes = o.Notes,
            OrderDetails = o.Details.Select(d => new OrderDetail
            {
                Id = d.Id,
                ProductId = d.ProductId,
                Quantity = d.Quantity,
                Price = d.UnitPrice,
                Product = new Product { Id = d.ProductId, Name = d.ProductName, ImageUrl = d.ImageUrl ?? "" }
            }).ToList()
        };

        return View(order);
    }

    [HttpPost]
    public async Task<IActionResult> ApplyCoupon(string code, decimal total)
    {
        var res = await _couponApiClient.ValidateCouponAsync(code, total);
        if (res.IsSuccess && res.Data != null)
        {
            // BE returns pre-computed DiscountAmount from ValidateCouponResult
            var discount = res.Data.DiscountAmount > 0
                ? res.Data.DiscountAmount
                : (total * res.Data.DiscountPercent / 100m);

            if (res.Data.MaxDiscountAmount > 0 && discount > res.Data.MaxDiscountAmount)
            {
                discount = res.Data.MaxDiscountAmount;
            }
            return Json(new { success = true, discount, message = $"Áp dụng voucher {code} thành công!" });
        }
        return Json(new { success = false, message = res.Message ?? "Mã giảm giá không hợp lệ" });
    }
}
