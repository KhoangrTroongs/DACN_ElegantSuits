using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NgoHuuDuc_2280600725.DTOs;
using NgoHuuDuc_2280600725.Services.Interfaces;

namespace NgoHuuDuc_2280600725.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
    public class CouponApiController : ControllerBase
    {
        private readonly ICouponService _couponService;
        private readonly ILogger<CouponApiController> _logger;

        public CouponApiController(ICouponService couponService, ILogger<CouponApiController> logger)
        {
            _couponService = couponService;
            _logger = logger;
        }

        // GET: api/CouponApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CouponDTO>>> GetCoupons()
        {
            try
            {
                var coupons = await _couponService.GetAllCouponsAsync();
                return Ok(coupons);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting coupons");
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách mã giảm giá" });
            }
        }

        // GET: api/CouponApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CouponDTO>> GetCoupon(int id)
        {
            try
            {
                var coupon = await _couponService.GetCouponByIdAsync(id);
                if (coupon == null)
                {
                    return NotFound(new { message = "Không tìm thấy mã giảm giá" });
                }
                return Ok(coupon);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting coupon");
                return StatusCode(500, new { message = "Lỗi khi lấy thông tin mã giảm giá" });
            }
        }

        // GET: api/CouponApi/code/{code}
        [HttpGet("code/{code}")]
        public async Task<ActionResult<CouponDTO>> GetCouponByCode(string code)
        {
            try
            {
                var coupon = await _couponService.GetCouponByCodeAsync(code);
                if (coupon == null)
                {
                    return NotFound(new { message = "Không tìm thấy mã giảm giá" });
                }
                return Ok(coupon);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting coupon by code");
                return StatusCode(500, new { message = "Lỗi khi lấy thông tin mã giảm giá" });
            }
        }

        // POST: api/CouponApi
        [HttpPost]
        public async Task<ActionResult<CouponDTO>> CreateCoupon([FromBody] CreateCouponDTO couponDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (await _couponService.CouponCodeExistsAsync(couponDto.Code))
                {
                    return BadRequest(new { message = "Mã giảm giá đã tồn tại" });
                }

                await _couponService.AddCouponAsync(couponDto);
                return CreatedAtAction(nameof(GetCouponByCode), new { code = couponDto.Code }, couponDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating coupon");
                return StatusCode(500, new { message = "Lỗi khi tạo mã giảm giá" });
            }
        }

        // PUT: api/CouponApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCoupon(int id, [FromBody] UpdateCouponDTO couponDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _couponService.UpdateCouponAsync(id, couponDto);
                if (result == null)
                {
                    return NotFound(new { message = "Không tìm thấy mã giảm giá để cập nhật" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating coupon");
                return StatusCode(500, new { message = "Lỗi khi cập nhật mã giảm giá" });
            }
        }

        // DELETE: api/CouponApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCoupon(int id)
        {
            try
            {
                var result = await _couponService.DeleteCouponAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Không tìm thấy mã giảm giá để xóa" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting coupon");
                return StatusCode(500, new { message = "Lỗi khi xóa mã giảm giá" });
            }
        }

        // POST: api/CouponApi/generate
        [HttpPost("generate")]
        public async Task<ActionResult<string>> GenerateCode()
        {
            try
            {
                var code = await _couponService.GenerateUniqueCouponCodeAsync();
                return Ok(new { code = code });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating coupon code");
                return StatusCode(500, new { message = "Lỗi khi tạo mã tự động" });
            }
        }
    }
}
