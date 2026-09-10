using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ElegantSuits.Domain.Entities;
using ElegantSuits.Domain.Enums;
using ElegantSuits.Web.Models;
using ElegantSuits.Web.Services;

namespace ElegantSuits.Web.Controllers;

[Authorize]
public class OrderController : Controller
{
    private readonly IOrderApiClient _orderApiClient;
    private readonly ILogger<OrderController> _logger;

    public OrderController(IOrderApiClient orderApiClient, ILogger<OrderController> logger)
    {
        _orderApiClient = orderApiClient;
        _logger = logger;
    }

    private string? GetToken() => HttpContext.Session.GetString("JwtToken");

    public async Task<IActionResult> Index()
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

    public async Task<IActionResult> Details(int id)
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
