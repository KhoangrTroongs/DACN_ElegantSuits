using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NgoHuuDuc_2280600725.Models;
using NgoHuuDuc_2280600725.Responsitories;

namespace NgoHuuDuc_2280600725.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        // GET: Category
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách tất cả danh mục và trả về view
            return View(await _categoryRepository.GetAllCategoriesAsync());
        }

        // GET: Category/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Lấy thông tin chi tiết danh mục theo id
            var category = await _categoryRepository.GetCategoryByIdAsync(id.Value);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Category/Create
        public IActionResult Create()
        {
            // Trả về view tạo mới danh mục
            return View();
        }

        // POST: Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description")] Category category)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(category.Name))
                {
                    ModelState.AddModelError("Name", "Tên danh mục không được để trống");
                    return View(category);
                }

                // Kiểm tra trùng tên danh mục
                var existingCategory = await _categoryRepository.GetCategoryByNameAsync(category.Name);
                if (existingCategory != null)
                {
                    ModelState.AddModelError("Name", "Tên danh mục đã tồn tại");
                    return View(category);
                }

                try
                {
                    await _categoryRepository.AddCategoryAsync(category);
                    TempData["Message"] = "Thêm danh mục thành công";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Bắt lỗi khi thêm danh mục và hiển thị thông báo lỗi
                    ModelState.AddModelError("", $"Đã xảy ra lỗi: {ex.Message}");
                }
            }
            return View(category);
        }

        // GET: Category/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Lấy thông tin danh mục cần sửa
            var category = await _categoryRepository.GetCategoryByIdAsync(id.Value);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Category category)
        {
            if (id != category.Id)
            {
                // Kiểm tra id truyền vào có khớp với id của danh mục không
                return BadRequest("ID không khớp với danh mục được sửa.");
            }

            if (!ModelState.IsValid)
            {
                return View(category);
            }

            try
            {
                if (string.IsNullOrEmpty(category.Name))
                {
                    ModelState.AddModelError("Name", "Tên danh mục không được để trống");
                    return View(category);
                }

                // Kiểm tra trùng tên danh mục (loại trừ chính nó)
                var existingCategory = await _categoryRepository.GetCategoryByNameAsync(category.Name, category.Id);
                if (existingCategory != null)
                {
                    ModelState.AddModelError("Name", "Tên danh mục đã tồn tại");
                    return View(category);
                }

                await _categoryRepository.UpdateCategoryAsync(category);
                TempData["Success"] = "Cập nhật danh mục thành công";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                // Bắt lỗi khi cập nhật danh mục
                ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật danh mục.");
            }
            return View(category);
        }

        // GET: Category/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Lấy thông tin danh mục cần xóa
            var category = await _categoryRepository.GetCategoryByIdAsync(id.Value);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Xóa danh mục theo id
            await _categoryRepository.DeleteCategoryAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
