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

    public IActionResult Index()
    {
        var list = new List<CouponDTO>();
        return View(list);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CreateCouponDTO dto)
    {
        if (ModelState.IsValid)
        {
            TempData["SuccessMessage"] = "Tạo mã giảm giá thành công!";
            return RedirectToAction(nameof(Index));
        }
        return View(dto);
    }

    [HttpGet]
    public IActionResult GenerateCode()
    {
        var code = "ES" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
        return Json(new { success = true, code });
    }

    public IActionResult Edit(int id)
    {
        var dto = new UpdateCouponDTO
        {
            Description = "Khuyến mãi",
            DiscountPercentage = 10,
            Quantity = 100
        };
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, UpdateCouponDTO dto)
    {
        if (ModelState.IsValid)
        {
            TempData["SuccessMessage"] = "Cập nhật mã giảm giá thành công!";
            return RedirectToAction(nameof(Index));
        }
        return View(dto);
    }

    public IActionResult Delete(int id)
    {
        var dto = new CouponDTO
        {
            Id = id,
            Code = "SAMPLE",
            DiscountPercentage = 10
        };
        return View(dto);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        TempData["SuccessMessage"] = "Xóa mã giảm giá thành công!";
        return RedirectToAction(nameof(Index));
    }
}
