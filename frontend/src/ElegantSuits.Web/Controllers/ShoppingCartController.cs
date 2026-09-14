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

    private string? GetToken()
    {
        var token = HttpContext.Session.GetString("JwtToken") ?? User.FindFirst("JwtToken")?.Value;
        if (!string.IsNullOrEmpty(token) && string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
        {
            HttpContext.Session.SetString("JwtToken", token);
        }
        return token;
    }

    public async Task<IActionResult> Index()
    {
        var token = GetToken();
        CartViewModel cart;

        if (!string.IsNullOrEmpty(token))
        {
            var res = await _cartApiClient.GetCartAsync(token);
            cart = res.Data ?? new CartViewModel();
        }
        else
        {
            cart = SessionCartService.GetSessionCart(HttpContext.Session);
        }

        // Heal any items that have missing details or missing IDs
        bool sessionUpdated = false;
        var validItems = new List<CartItemViewModel>();
        int nextId = 1;
        foreach (var item in cart.Items)
        {
            if (item.Id <= 0)
            {
                item.Id = nextId;
                sessionUpdated = true;
            }
            nextId = Math.Max(nextId, item.Id + 1);

            if (string.IsNullOrEmpty(item.ProductName) || item.Price <= 0 || string.IsNullOrEmpty(item.ImageUrl))
            {
                try
                {
                    var p = await _productApiClient.GetProductByIdAsync(item.ProductId);
                    if (p != null)
                    {
                        item.ProductName = p.Name;
                        item.Price = p.Price;
                        item.ImageUrl = !string.IsNullOrEmpty(p.ImageUrl) ? p.ImageUrl : "/images/no-image.svg";
                        sessionUpdated = true;
                    }
                }
                catch { }
            }

            // Only keep items that have a valid name and price
            if (!string.IsNullOrEmpty(item.ProductName) && item.Price > 0)
            {
                validItems.Add(item);
            }
            else
            {
                sessionUpdated = true;
            }
        }

        cart.Items = validItems;

        if (sessionUpdated && string.IsNullOrEmpty(token))
        {
            SessionCartService.SaveSessionCart(HttpContext.Session, cart);
        }

        var cartItems = cart.Items.Select(i => new CartItem
        {
            Id = i.Id,
            ProductId = i.ProductId,
            Quantity = i.Quantity,
            Size = i.Size,
            ProductName = !string.IsNullOrEmpty(i.ProductName) ? i.ProductName : "Sản phẩm",
            Price = i.Price,
            ImageUrl = !string.IsNullOrEmpty(i.ImageUrl) ? i.ImageUrl : "/images/no-image.svg",
            Product = new Product
            {
                Id = i.ProductId,
                Name = !string.IsNullOrEmpty(i.ProductName) ? i.ProductName : "Sản phẩm",
                Price = i.Price,
                ImageUrl = !string.IsNullOrEmpty(i.ImageUrl) ? i.ImageUrl : "/images/no-image.svg"
            }
        }).ToList();

        ViewBag.TotalPrice = cart.TotalPrice > 0 ? cart.TotalPrice : cartItems.Sum(x => x.Price * x.Quantity);
        ViewBag.Discount = cart.Discount;
        return View(cartItems);
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart(int productId, int quantity = 1, string? size = null)
    {
        var token = GetToken();
        bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        int totalCartCount = 0;

        if (!string.IsNullOrEmpty(token))
        {
            var req = new AddToCartRequest
            {
                ProductId = productId,
                Quantity = quantity,
                Size = size
            };
            var res = await _cartApiClient.AddToCartAsync(req, token);
            if (res.IsSuccess)
            {
                var cartRes = await _cartApiClient.GetCartAsync(token);
                totalCartCount = cartRes.Data?.Items.Sum(i => i.Quantity) ?? 0;
                if (isAjax)
                {
                    return Json(new { success = true, count = totalCartCount, message = "Đã thêm vào giỏ hàng thành công!" });
                }
                TempData["SuccessMessage"] = "Đã thêm sản phẩm vào giỏ hàng!";
            }
            else
            {
                if (isAjax)
                {
                    return Json(new { success = false, message = res.Message ?? "Không thể thêm vào giỏ hàng." });
                }
                TempData["ErrorMessage"] = res.Message ?? "Không thể thêm vào giỏ hàng.";
            }
        }
        else
        {
            var p = await _productApiClient.GetProductByIdAsync(productId);
            if (p != null)
            {
                var product = new ProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl
                };
                SessionCartService.AddToCart(HttpContext.Session, product, quantity, size);
                totalCartCount = SessionCartService.GetCount(HttpContext.Session);
                if (isAjax)
                {
                    return Json(new { success = true, count = totalCartCount, message = "Đã thêm vào giỏ hàng thành công!" });
                }
                TempData["SuccessMessage"] = "Đã thêm sản phẩm vào giỏ hàng!";
            }
            else
            {
                if (isAjax)
                {
                    return Json(new { success = false, message = "Sản phẩm không tồn tại." });
                }
                TempData["ErrorMessage"] = "Sản phẩm không tồn tại.";
            }
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> UpdateCart(int id, int quantity, int? productId = null)
    {
        var token = GetToken();
        int cartCount = 0;
        if (!string.IsNullOrEmpty(token))
        {
            var res = await _cartApiClient.UpdateCartItemAsync(id, quantity, token);
            if (!res.IsSuccess && productId.HasValue)
            {
                var cartRes = await _cartApiClient.GetCartAsync(token);
                var item = cartRes.Data?.Items.FirstOrDefault(x => x.ProductId == productId.Value);
                if (item != null)
                {
                    res = await _cartApiClient.UpdateCartItemAsync(item.Id, quantity, token);
                }
            }
            var fresh = await _cartApiClient.GetCartAsync(token);
            cartCount = fresh.Data?.Items.Sum(x => x.Quantity) ?? 0;
            return Json(new { success = res.IsSuccess, cartCount, message = res.Message });
        }
        else
        {
            var cart = SessionCartService.GetSessionCart(HttpContext.Session);
            var item = cart.Items.FirstOrDefault(x => (id > 0 && x.Id == id) || (productId.HasValue && x.ProductId == productId.Value));
            if (item != null)
            {
                SessionCartService.UpdateQuantity(HttpContext.Session, item.Id, quantity, productId);
            }
            cartCount = SessionCartService.GetCount(HttpContext.Session);
            return Json(new { success = true, cartCount, message = "Đã cập nhật số lượng thành công!" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateQuantity(int productId, int quantity, int? id = null)
    {
        return await UpdateCart(id ?? 0, quantity, productId);
    }

    [HttpPost]
    public async Task<IActionResult> RemoveFromCart(int id, int? productId = null)
    {
        var token = GetToken();
        bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        int cartCount = 0;

        if (!string.IsNullOrEmpty(token))
        {
            var res = await _cartApiClient.RemoveCartItemAsync(id, token);
            if (!res.IsSuccess && productId.HasValue)
            {
                var cartRes = await _cartApiClient.GetCartAsync(token);
                var item = cartRes.Data?.Items.FirstOrDefault(x => x.ProductId == productId.Value || x.Id == id);
                if (item != null)
                {
                    res = await _cartApiClient.RemoveCartItemAsync(item.Id, token);
                }
            }
            var fresh = await _cartApiClient.GetCartAsync(token);
            cartCount = fresh.Data?.Items.Sum(x => x.Quantity) ?? 0;
            if (isAjax) return Json(new { success = res.IsSuccess, cartCount, message = res.Message });
        }
        else
        {
            var cart = SessionCartService.GetSessionCart(HttpContext.Session);
            var item = cart.Items.FirstOrDefault(x => (id > 0 && x.Id == id) || (productId.HasValue && x.ProductId == productId.Value));
            if (item != null)
            {
                SessionCartService.RemoveItem(HttpContext.Session, item.Id, productId);
            }
            else if (productId.HasValue)
            {
                SessionCartService.RemoveItem(HttpContext.Session, 0, productId.Value);
            }
            cartCount = SessionCartService.GetCount(HttpContext.Session);
            if (isAjax) return Json(new { success = true, cartCount, message = "Đã xóa sản phẩm khỏi giỏ hàng!" });
        }

        TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi giỏ hàng.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> ClearCart()
    {
        var token = GetToken();
        if (!string.IsNullOrEmpty(token))
        {
            await _cartApiClient.ClearCartAsync(token);
        }
        else
        {
            SessionCartService.Clear(HttpContext.Session);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetCartCount()
    {
        var token = GetToken();
        if (!string.IsNullOrEmpty(token))
        {
            var res = await _cartApiClient.GetCartAsync(token);
            int count = res.Data?.Items.Sum(i => i.Quantity) ?? 0;
            return Json(new { count });
        }

        int sessionCount = SessionCartService.GetCount(HttpContext.Session);
        return Json(new { count = sessionCount });
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

        var mapped = orders.Select(o =>
        {
            var orderStatus = Enum.TryParse<OrderStatus>(o.OrderStatus, out var os) ? os
                : (Enum.TryParse<OrderStatus>(o.Status, out var s) ? s : OrderStatus.Pending);
            var paymentStatus = Enum.TryParse<PaymentStatus>(o.PaymentStatus, out var ps) ? ps : PaymentStatus.Pending;

            return new Order
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                TotalPrice = o.TotalPrice,
                TotalAmount = o.TotalPrice,
                PaymentMethod = o.PaymentMethod,
                PaymentStatus = paymentStatus,
                OrderStatus = orderStatus,
                Status = orderStatus,
                ShippingAddress = o.ShippingAddress,
                Notes = o.Notes
            };
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
        var orderStatus = Enum.TryParse<OrderStatus>(o.OrderStatus, out var os) ? os
            : (Enum.TryParse<OrderStatus>(o.Status, out var s) ? s : OrderStatus.Pending);
        var paymentStatus = Enum.TryParse<PaymentStatus>(o.PaymentStatus, out var ps) ? ps : PaymentStatus.Pending;

        var order = new Order
        {
            Id = o.Id,
            OrderDate = o.OrderDate,
            TotalPrice = o.TotalPrice,
            TotalAmount = o.TotalPrice,
            PaymentMethod = o.PaymentMethod,
            PaymentStatus = paymentStatus,
            OrderStatus = orderStatus,
            Status = orderStatus,
            ShippingAddress = o.ShippingAddress,
            Notes = o.Notes,
            OrderDetails = o.Details.Select(d => new OrderDetail
            {
                Id = d.Id,
                OrderId = o.Id,
                ProductId = d.ProductId,
                Quantity = d.Quantity,
                Price = d.UnitPrice,
                Size = d.Size,
                Product = new Product
                {
                    Id = d.ProductId,
                    Name = !string.IsNullOrEmpty(d.ProductName) ? d.ProductName : "Sản phẩm",
                    ImageUrl = d.ImageUrl ?? "/images/no-image.svg"
                }
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
