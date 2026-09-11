using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ElegantSuits.Domain.Entities;
using ElegantSuits.Domain.Enums;
using ElegantSuits.Web.Models;
using ElegantSuits.Web.Services;

namespace ElegantSuits.Web.Controllers;

[Authorize(Roles = "Administrator")]
public class OrderController : Controller
{
    private readonly IOrderApiClient _orderApiClient;
    private readonly ILogger<OrderController> _logger;

    public OrderController(IOrderApiClient orderApiClient, ILogger<OrderController> logger)
    {
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
        var res = await _orderApiClient.GetAllOrdersAsync(token);
        var orders = res.Data ?? new List<OrderViewModel>();

        var mapped = orders.Select(o =>
        {
            var orderStatus = Enum.TryParse<OrderStatus>(o.OrderStatus, out var os) ? os
                : (Enum.TryParse<OrderStatus>(o.Status, out var s) ? s : OrderStatus.Pending);
            var paymentStatus = Enum.TryParse<PaymentStatus>(o.PaymentStatus, out var ps) ? ps : PaymentStatus.Pending;
            var displayName = !string.IsNullOrEmpty(o.CustomerName) ? o.CustomerName
                : (!string.IsNullOrEmpty(o.UserName) ? o.UserName : "Khách hàng");

            return new Order
            {
                Id = o.Id,
                UserId = o.UserId,
                User = new ApplicationUser
                {
                    Id = o.UserId ?? "",
                    UserName = displayName,
                    FullName = displayName
                },
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

    public async Task<IActionResult> Details(int id)
    {
        var token = GetToken();
        var res = await _orderApiClient.GetOrderByIdAsync(id, token);
        if (!res.IsSuccess || res.Data == null) return NotFound();

        var o = res.Data;
        var orderStatus = Enum.TryParse<OrderStatus>(o.OrderStatus, out var os) ? os
            : (Enum.TryParse<OrderStatus>(o.Status, out var s) ? s : OrderStatus.Pending);
        var paymentStatus = Enum.TryParse<PaymentStatus>(o.PaymentStatus, out var ps) ? ps : PaymentStatus.Pending;
        var displayName = !string.IsNullOrEmpty(o.CustomerName) ? o.CustomerName
            : (!string.IsNullOrEmpty(o.UserName) ? o.UserName : "Khách hàng");

        var order = new Order
        {
            Id = o.Id,
            UserId = o.UserId,
            User = new ApplicationUser
            {
                Id = o.UserId ?? "",
                UserName = displayName,
                FullName = displayName
            },
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
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
    {
        var token = GetToken();
        var res = await _orderApiClient.UpdateOrderStatusAsync(id, (int)status, token);
        if (res.IsSuccess)
        {
            TempData["SuccessMessage"] = "Cập nhật trạng thái đơn hàng thành công!";
        }
        else
        {
            TempData["ErrorMessage"] = res.Message ?? "Không thể cập nhật trạng thái đơn hàng.";
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [AllowAnonymous]
    public IActionResult OrderSuccess(int id)
    {
        return View(id);
    }

    [AllowAnonymous]
    public IActionResult OrderFailed(int id)
    {
        return View(id);
    }
}
