using ElegantSuits.Application.Common.Interfaces;
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
    private readonly ICouponRepository _couponRepository;

    public CouponApiController(ISender sender, ICouponRepository couponRepository)
    {
        _sender = sender;
        _couponRepository = couponRepository;
    }

    // GET: api/CouponApi
    [HttpGet]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
    public async Task<ActionResult<ResponseDTO<IEnumerable<CouponDTO>>>> GetCoupons(CancellationToken cancellationToken)
    {
        var result = await _couponRepository.GetAllCouponsAsync(cancellationToken);
        return Ok(ResponseDTO<IEnumerable<CouponDTO>>.Success(result));
    }

    // GET: api/CouponApi/{id}
    [HttpGet("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
    public async Task<ActionResult<ResponseDTO<CouponDTO>>> GetCouponById(int id, CancellationToken cancellationToken)
    {
        var coupon = await _couponRepository.GetCouponByIdAsync(id, cancellationToken);
        if (coupon == null)
        {
            return NotFound(ResponseDTO<CouponDTO>.Fail("Không tìm thấy mã giảm giá"));
        }
        return Ok(ResponseDTO<CouponDTO>.Success(coupon));
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
        try
        {
            var result = await _couponRepository.AddCouponAsync(dto, cancellationToken);
            return Ok(ResponseDTO<CouponDTO>.Success(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ResponseDTO<CouponDTO>.Fail(ex.Message));
        }
    }

    // PUT: api/CouponApi/{id}
    [HttpPut("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
    public async Task<ActionResult<ResponseDTO<CouponDTO>>> UpdateCoupon(
        int id,
        [FromBody] UpdateCouponDTO dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _couponRepository.UpdateCouponAsync(id, dto, cancellationToken);
            if (result == null)
            {
                return NotFound(ResponseDTO<CouponDTO>.Fail("Không tìm thấy mã giảm giá"));
            }
            return Ok(ResponseDTO<CouponDTO>.Success(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ResponseDTO<CouponDTO>.Fail(ex.Message));
        }
    }

    // DELETE: api/CouponApi/{id}
    [HttpDelete("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
    public async Task<ActionResult<ResponseDTO<bool>>> DeleteCoupon(int id, CancellationToken cancellationToken)
    {
        try
        {
            var success = await _couponRepository.DeleteCouponAsync(id, cancellationToken);
            if (!success)
            {
                return NotFound(ResponseDTO<bool>.Fail("Không tìm thấy mã giảm giá để xóa"));
            }
            return Ok(ResponseDTO<bool>.Success(true));
        }
        catch (Exception ex)
        {
            return BadRequest(ResponseDTO<bool>.Fail(ex.Message));
        }
    }
}
