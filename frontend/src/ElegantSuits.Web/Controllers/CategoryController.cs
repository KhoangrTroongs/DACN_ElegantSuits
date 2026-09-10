using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ElegantSuits.Domain.Entities;
using ElegantSuits.Web.Models;
using ElegantSuits.Web.Services;

namespace ElegantSuits.Web.Controllers;

[Authorize(Roles = "Administrator")]
public class CategoryController : Controller
{
    private readonly ICategoryApiClient _categoryApiClient;

    public CategoryController(ICategoryApiClient categoryApiClient)
    {
        _categoryApiClient = categoryApiClient;
    }

    public async Task<IActionResult> Index()
    {
        var res = await _categoryApiClient.GetAllCategoriesAsync();
        var list = (res.Data ?? new List<CategoryViewModel>()).Select(c => new Category
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description ?? ""
        }).ToList();

        return View(list);
    }

    public async Task<IActionResult> Details(int id)
    {
        var res = await _categoryApiClient.GetCategoryByIdAsync(id);
        if (!res.IsSuccess || res.Data == null) return NotFound();

        var c = res.Data;
        var category = new Category
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description ?? ""
        };
        return View(category);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        if (ModelState.IsValid)
        {
            var vm = new CategoryViewModel
            {
                Name = category.Name,
                Description = category.Description
            };
            var res = await _categoryApiClient.CreateCategoryAsync(vm);
            if (res.IsSuccess)
            {
                TempData["SuccessMessage"] = "Tạo danh mục thành công!";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", res.Message ?? "Lỗi tạo danh mục.");
        }
        return View(category);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var res = await _categoryApiClient.GetCategoryByIdAsync(id);
        if (!res.IsSuccess || res.Data == null) return NotFound();

        var c = res.Data;
        var category = new Category
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description ?? ""
        };
        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Category category)
    {
        if (id != category.Id) return BadRequest();

        if (ModelState.IsValid)
        {
            var vm = new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };
            var res = await _categoryApiClient.UpdateCategoryAsync(id, vm);
            if (res.IsSuccess)
            {
                TempData["SuccessMessage"] = "Cập nhật danh mục thành công!";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", res.Message ?? "Lỗi cập nhật danh mục.");
        }
        return View(category);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var res = await _categoryApiClient.GetCategoryByIdAsync(id);
        if (!res.IsSuccess || res.Data == null) return NotFound();

        var c = res.Data;
        var category = new Category
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description ?? ""
        };
        return View(category);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var res = await _categoryApiClient.DeleteCategoryAsync(id);
        if (res.IsSuccess)
        {
            TempData["SuccessMessage"] = "Xóa danh mục thành công!";
        }
        else
        {
            TempData["ErrorMessage"] = res.Message ?? "Lỗi xóa danh mục.";
        }
        return RedirectToAction(nameof(Index));
    }
}
