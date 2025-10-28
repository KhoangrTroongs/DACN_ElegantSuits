using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NgoHuuDuc_2280600725.DTOs;
using NgoHuuDuc_2280600725.Services.Interfaces;

namespace NgoHuuDuc_2280600725.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class CouponController : Controller
    {
        private readonly ICouponService _couponService;
        private readonly ILogger<CouponController> _logger;

        public CouponController(
            ICouponService couponService,
            ILogger<CouponController> logger)
        {
            _couponService = couponService;
            _logger = logger;
        }

        // GET: Coupon
        public async Task<IActionResult> Index()
        {
            try
            {
                var coupons = await _couponService.GetAllCouponsAsync();
                return View(coupons);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all coupons");
                TempData["ErrorMessage"] = "Lỗi khi tải danh sách mã giảm giá";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Coupon/Create
        public IActionResult Create()
        {
            return View();
        }

        // GET: Coupon/GenerateCode (API endpoint for AJAX)
        [HttpGet]
        public async Task<IActionResult> GenerateCode()
        {
            try
            {
                var code = await _couponService.GenerateUniqueCouponCodeAsync();
                return Json(new { success = true, code = code });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating coupon code");
                return Json(new { success = false, message = "Lỗi khi tạo mã giảm giá" });
            }
        }

        // POST: Coupon/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCouponDTO couponDto)
        {
            try
            {
                // Auto-generate code if empty
                if (string.IsNullOrWhiteSpace(couponDto.Code))
                {
                    couponDto.Code = await _couponService.GenerateUniqueCouponCodeAsync();
                }

                // Validate ExpiryDate based on Quantity
                if (couponDto.Quantity != -1 && !couponDto.ExpiryDate.HasValue)
                {
                    ModelState.AddModelError("ExpiryDate", "Ngày hết hạn không được để trống khi số lượng không phải là 'Không giới hạn'");
                }

                if (!ModelState.IsValid)
                {
                    return View(couponDto);
                }

                // Check if coupon code already exists
                if (await _couponService.CouponCodeExistsAsync(couponDto.Code))
                {
                    ModelState.AddModelError("Code", "Mã giảm giá này đã tồn tại");
                    return View(couponDto);
                }

                await _couponService.AddCouponAsync(couponDto);
                TempData["SuccessMessage"] = "Tạo mã giảm giá thành công";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating coupon");
                ModelState.AddModelError("", "Lỗi khi tạo mã giảm giá");
                return View(couponDto);
            }
        }

        // GET: Coupon/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var coupon = await _couponService.GetCouponByIdAsync(id.Value);
                if (coupon == null)
                {
                    return NotFound();
                }

                var updateDto = new UpdateCouponDTO
                {
                    Quantity = coupon.Quantity,
                    DiscountPercentage = coupon.DiscountPercentage,
                    ExpiryDate = coupon.ExpiryDate,
                    IsActive = coupon.IsActive
                };

                ViewData["CouponCode"] = coupon.Code;
                return View(updateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting coupon for edit");
                TempData["ErrorMessage"] = "Lỗi khi tải thông tin mã giảm giá";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Coupon/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateCouponDTO couponDto)
        {
            // Validate ExpiryDate based on Quantity
            if (couponDto.Quantity != -1 && !couponDto.ExpiryDate.HasValue)
            {
                ModelState.AddModelError("ExpiryDate", "Ngày hết hạn không được để trống khi số lượng không phải là 'Không giới hạn'");
            }

            if (!ModelState.IsValid)
            {
                var coupon = await _couponService.GetCouponByIdAsync(id);
                if (coupon != null)
                {
                    ViewData["CouponCode"] = coupon.Code;
                }
                return View(couponDto);
            }

            try
            {
                var result = await _couponService.UpdateCouponAsync(id, couponDto);
                if (result == null)
                {
                    return NotFound();
                }

                TempData["SuccessMessage"] = "Cập nhật mã giảm giá thành công";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating coupon");
                ModelState.AddModelError("", "Lỗi khi cập nhật mã giảm giá");
                var coupon = await _couponService.GetCouponByIdAsync(id);
                if (coupon != null)
                {
                    ViewData["CouponCode"] = coupon.Code;
                }
                return View(couponDto);
            }
        }

        // GET: Coupon/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var coupon = await _couponService.GetCouponByIdAsync(id.Value);
                if (coupon == null)
                {
                    return NotFound();
                }

                return View(coupon);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting coupon for delete");
                TempData["ErrorMessage"] = "Lỗi khi tải thông tin mã giảm giá";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Coupon/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _couponService.DeleteCouponAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                TempData["SuccessMessage"] = "Xóa mã giảm giá thành công";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting coupon");
                TempData["ErrorMessage"] = "Lỗi khi xóa mã giảm giá";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Coupon/GetAvailableCoupons (API endpoint)
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableCoupons([FromBody] GetAvailableCouponsRequest request)
        {
            try
            {
                if (request == null || request.CartTotal <= 0 || request.ProductIds == null || request.ProductIds.Count == 0)
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ", coupons = new List<object>() });
                }

                var availableCoupons = await _couponService.GetAvailableCouponsAsync(
                    request.CartTotal,
                    request.HasFreeship,
                    request.ProductIds);

                if (availableCoupons.Count == 0)
                {
                    return Json(new { success = true, message = "Không có mã giảm giá khả dụng", coupons = new List<object>() });
                }

                var couponList = availableCoupons.Select(c => new
                {
                    c.Id,
                    c.Code,
                    c.DiscountPercentage,
                    c.ExpiryDate,
                    SavingAmount = Math.Round(request.CartTotal * (c.DiscountPercentage / 100), 0)
                }).ToList();

                return Json(new { success = true, message = "Lấy danh sách mã giảm giá thành công", coupons = couponList });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available coupons");
                return Json(new { success = false, message = "Lỗi khi lấy danh sách mã giảm giá", coupons = new List<object>() });
            }
        }
    }

    public class GetAvailableCouponsRequest
    {
        public decimal CartTotal { get; set; }
        public bool HasFreeship { get; set; }
        public List<int> ProductIds { get; set; } = new List<int>();
    }
}

