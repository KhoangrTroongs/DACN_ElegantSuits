using System.Security.Claims;
using ElegantSuits.Application.Common.Models;
using ElegantSuits.Application.Features.Cart.Commands.AddToCart;
using ElegantSuits.Application.Features.Cart.Commands.ClearCart;
using ElegantSuits.Application.Features.Cart.Commands.RemoveCartItem;
using ElegantSuits.Application.Features.Cart.Commands.UpdateCartItem;
using ElegantSuits.Application.Features.Cart.Contracts;
using ElegantSuits.Application.Features.Cart.Queries.GetCart;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElegantSuits.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class CartController : ControllerBase
{
    private readonly ISender _sender;

    public CartController(ISender sender)
    {
        _sender = sender;
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    // GET: api/Cart
    [HttpGet]
    public async Task<ActionResult<ResponseDTO<CartDTO>>> GetCart(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ResponseDTO<CartDTO>.Fail("User not authenticated."));
        }

        var cart = await _sender.Send(new GetCartQuery(userId), cancellationToken);
        if (cart == null)
        {
            return Ok(ResponseDTO<CartDTO>.Success(new CartDTO
            {
                UserId = userId,
                Items = new List<CartItemDTO>(),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            }));
        }

        return Ok(ResponseDTO<CartDTO>.Success(cart));
    }

    // POST: api/Cart
    [HttpPost]
    public async Task<ActionResult<ResponseDTO<CartDTO>>> AddToCart(
        [FromBody] AddToCartDTO dto,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ResponseDTO<CartDTO>.Fail("User not authenticated."));
        }

        var result = await _sender.Send(new AddToCartCommand(userId, dto), cancellationToken);
        return Ok(ResponseDTO<CartDTO>.Success(result));
    }

    // PUT: api/Cart
    [HttpPut]
    public async Task<ActionResult<ResponseDTO<CartDTO>>> UpdateCartItem(
        [FromBody] UpdateCartItemDTO dto,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ResponseDTO<CartDTO>.Fail("User not authenticated."));
        }

        var result = await _sender.Send(new UpdateCartItemCommand(userId, dto), cancellationToken);
        if (result == null)
        {
            return NotFound(ResponseDTO<CartDTO>.Fail("Cart not found."));
        }

        return Ok(ResponseDTO<CartDTO>.Success(result));
    }

    // PUT: api/Cart/item/5 or api/Cart/items/5
    [HttpPut("item/{cartItemId:int}")]
    [HttpPut("items/{cartItemId:int}")]
    public async Task<ActionResult<ResponseDTO<CartDTO>>> UpdateCartItemRoute(
        int cartItemId,
        [FromBody] UpdateCartItemDTO dto,
        CancellationToken cancellationToken)
    {
        dto.CartItemId = cartItemId;
        return await UpdateCartItem(dto, cancellationToken);
    }

    // DELETE: api/Cart/item/5 or api/Cart/items/5
    [HttpDelete("item/{cartItemId:int}")]
    [HttpDelete("items/{cartItemId:int}")]
    public async Task<ActionResult<ResponseDTO<bool>>> RemoveCartItem(
        int cartItemId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ResponseDTO<bool>.Fail("User not authenticated."));
        }

        var result = await _sender.Send(new RemoveCartItemCommand(userId, cartItemId), cancellationToken);
        if (!result)
        {
            return NotFound(ResponseDTO<bool>.Fail("Item not found in cart."));
        }

        return Ok(ResponseDTO<bool>.Success(true, "Item removed successfully."));
    }

    // DELETE: api/Cart or api/Cart/clear
    [HttpDelete]
    [HttpDelete("clear")]
    public async Task<ActionResult<ResponseDTO<bool>>> ClearCart(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ResponseDTO<bool>.Fail("User not authenticated."));
        }

        var result = await _sender.Send(new ClearCartCommand(userId), cancellationToken);
        return Ok(ResponseDTO<bool>.Success(result, "Cart cleared successfully."));
    }
}
