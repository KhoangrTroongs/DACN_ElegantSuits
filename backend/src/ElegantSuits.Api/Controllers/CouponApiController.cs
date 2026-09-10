using ElegantSuits.Application.Common.Models;
using ElegantSuits.Application.Features.Coupons.Commands.CreateCoupon;
using ElegantSuits.Application.Features.Coupons.Contracts;
using ElegantSuits.Application.Features.Coupons.Queries.GetCoupons;
using ElegantSuits.Application.Features.Coupons.Queries.ValidateCoupon;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElegantSuits.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CouponApiController : ControllerBase
{
    private readonly ISender _sender;

    public CouponApiController(ISender sender)
    {
        _sender = sender;
    }

    // GET: api/CouponApi
    [HttpGet]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
    public async Task<ActionResult<IEnumerable<CouponDTO>>> GetCoupons(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCouponsQuery(), cancellationToken);
        return Ok(result);
    }

    // POST: api/CouponApi/validate
    [HttpPost("validate")]
    public async Task<ActionResult<ResponseDTO<ValidateCouponResult>>> ValidateCoupon(
        [FromBody] ValidateCouponRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ValidateCouponQuery(request.Code, request.OrderAmount), cancellationToken);
        if (!result.IsValid)
        {
            return BadRequest(ResponseDTO<ValidateCouponResult>.Fail(result.Message));
        }

        return Ok(ResponseDTO<ValidateCouponResult>.Success(result));
    }

    // POST: api/CouponApi
    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
    public async Task<ActionResult<ResponseDTO<CouponDTO>>> CreateCoupon(
        [FromBody] CreateCouponDTO dto,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreateCouponCommand(dto), cancellationToken);
        return Ok(ResponseDTO<CouponDTO>.Success(result));
    }
}
