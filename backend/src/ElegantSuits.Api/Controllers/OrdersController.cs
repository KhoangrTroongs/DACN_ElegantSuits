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

    public OrdersController(ISender sender)
    {
        _sender = sender;
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
