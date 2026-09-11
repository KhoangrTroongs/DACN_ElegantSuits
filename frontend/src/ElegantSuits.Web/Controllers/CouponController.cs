using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ElegantSuits.Web.Models;
using ElegantSuits.Web.Services;

namespace ElegantSuits.Web.Controllers;

[Authorize(Roles = "Administrator")]
public class CouponController : Controller
{
    private readonly ICouponApiClient _couponApiClient;
    private readonly ILogger<CouponController> _logger;

    public CouponController(
        ICouponApiClient couponApiClient,
        ILogger<CouponController> logger)
    {
        _couponApiClient = couponApiClient;
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
        var res = await _couponApiClient.GetAllCouponsAsync(token);
        var list = res.Data ?? new List<CouponDTO>();
        return View(list);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCouponDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Code))
        {
            dto.Code = "ES" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
        }
        else
        {
            dto.Code = dto.Code.Trim().ToUpper();
        }

        if (!dto.ExpiryDate.HasValue)
        {
            dto.ExpiryDate = DateTime.Now.AddDays(30);
        }

        if (ModelState.IsValid)
        {
            var token = GetToken();
            var res = await _couponApiClient.CreateCouponAsync(dto, token);
            if (res.IsSuccess)
            {
                TempData["SuccessMessage"] = "Tạo mã giảm giá thành công!";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = res.Message ?? "Không thể tạo mã giảm giá.";
        }
        return View(dto);
    }

    [HttpGet]
    public IActionResult GenerateCode()
    {
        var code = "ES" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
        return Json(new { success = true, code });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var token = GetToken();
        var res = await _couponApiClient.GetCouponByIdAsync(id, token);
        if (!res.IsSuccess || res.Data == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy mã giảm giá.";
            return RedirectToAction(nameof(Index));
        }

        var coupon = res.Data;
        ViewData["CouponCode"] = coupon.Code;
        var dto = new UpdateCouponDTO
        {
            Code = coupon.Code,
            Description = coupon.Description,
            DiscountPercentage = coupon.DiscountPercentage,
            Quantity = coupon.Quantity,
            MinimumAmount = coupon.MinimumAmount,
            ExpiryDate = coupon.ExpiryDate,
            IsActive = coupon.IsActive
        };
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateCouponDTO dto)
    {
        if (!dto.ExpiryDate.HasValue)
        {
            dto.ExpiryDate = DateTime.Now.AddDays(30);
        }

        if (ModelState.IsValid)
        {
            var token = GetToken();
            var res = await _couponApiClient.UpdateCouponAsync(id, dto, token);
            if (res.IsSuccess)
            {
                TempData["SuccessMessage"] = "Cập nhật mã giảm giá thành công!";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = res.Message ?? "Không thể cập nhật mã giảm giá.";
        }

        var existing = await _couponApiClient.GetCouponByIdAsync(id, GetToken());
        ViewData["CouponCode"] = existing?.Data?.Code ?? dto.Code;

        return View(dto);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var token = GetToken();
        var res = await _couponApiClient.GetCouponByIdAsync(id, token);
        if (!res.IsSuccess || res.Data == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy mã giảm giá.";
            return RedirectToAction(nameof(Index));
        }
        return View(res.Data);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var token = GetToken();
        var res = await _couponApiClient.DeleteCouponAsync(id, token);
        if (res.IsSuccess)
        {
            TempData["SuccessMessage"] = "Xóa mã giảm giá thành công!";
        }
        else
        {
            TempData["ErrorMessage"] = res.Message ?? "Không thể xóa mã giảm giá.";
        }
        return RedirectToAction(nameof(Index));
    }
}
