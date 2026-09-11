using System.Security.Claims;
using ElegantSuits.Application.Common.Models;
using ElegantSuits.Application.Features.Orders.Commands.CreateOrder;
using ElegantSuits.Application.Features.Orders.Contracts;
using ElegantSuits.Application.Features.Orders.Queries.GetAllOrders;
using ElegantSuits.Application.Features.Orders.Queries.GetUserOrders;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElegantSuits.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class OrdersController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ElegantSuits.Application.Common.Interfaces.IOrderRepository _orderRepository;

    public OrdersController(
        ISender sender,
        ElegantSuits.Application.Common.Interfaces.IOrderRepository orderRepository)
    {
        _sender = sender;
        _orderRepository = orderRepository;
    }

    // GET: api/Orders
    [HttpGet]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
    public async Task<ActionResult<ResponseDTO<IEnumerable<OrderDTO>>>> GetAllOrders(CancellationToken cancellationToken)
    {
        var orders = await _sender.Send(new GetAllOrdersQuery(), cancellationToken);
        return Ok(ResponseDTO<IEnumerable<OrderDTO>>.Success(orders));
    }

    // GET: api/Orders/my-orders
    [HttpGet("my-orders")]
    public async Task<ActionResult<ResponseDTO<IEnumerable<OrderDTO>>>> GetMyOrders(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ResponseDTO<IEnumerable<OrderDTO>>.Fail("User not authenticated."));
        }

        var orders = await _sender.Send(new GetUserOrdersQuery(userId), cancellationToken);
        return Ok(ResponseDTO<IEnumerable<OrderDTO>>.Success(orders));
    }

    // GET: api/Orders/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<ResponseDTO<OrderDTO>>> GetOrderById(int id, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetOrderByIdAsync(id, cancellationToken);
        if (order == null)
        {
            return NotFound(ResponseDTO<OrderDTO>.Fail("Đơn hàng không tồn tại."));
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Administrator");
        if (!isAdmin && order.UserId != userId)
        {
            return Forbid();
        }

        return Ok(ResponseDTO<OrderDTO>.Success(order));
    }

    // PUT: api/Orders/{id}/status
    [HttpPut("{id}/status")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
    public async Task<ActionResult<ResponseDTO<bool>>> UpdateOrderStatus(
        int id,
        [FromBody] UpdateOrderStatusDTO dto,
        CancellationToken cancellationToken)
    {
        var updated = await _orderRepository.UpdateOrderStatusAsync(id, dto.Status, cancellationToken);
        if (!updated)
        {
            return NotFound(ResponseDTO<bool>.Fail("Không tìm thấy đơn hàng."));
        }

        return Ok(ResponseDTO<bool>.Success(true, "Cập nhật trạng thái đơn hàng thành công."));
    }

    // POST: api/Orders
    [HttpPost]
    public async Task<ActionResult<ResponseDTO<OrderDTO>>> CreateOrder(
        [FromBody] CreateOrderDTO dto,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ResponseDTO<OrderDTO>.Fail("User not authenticated."));
        }

        var result = await _sender.Send(new CreateOrderCommand(userId, dto), cancellationToken);
        return Ok(ResponseDTO<OrderDTO>.Success(result));
    }
}
