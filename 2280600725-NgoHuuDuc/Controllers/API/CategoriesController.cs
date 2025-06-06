using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NgoHuuDuc_2280600725.DTOs;
using NgoHuuDuc_2280600725.Services.Interfaces;

namespace NgoHuuDuc_2280600725.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(
            ICategoryService categoryService,
            ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        // GET: api/Categories
        [HttpGet]
        public async Task<ActionResult<ResponseDTO<IEnumerable<CategoryDTO>>>> GetCategories()
        {
            try
            {
                // Lấy tất cả danh mục và trả về cho client
                var categories = await _categoryService.GetAllCategoriesAsync();
                return Ok(ResponseDTO<IEnumerable<CategoryDTO>>.Success(categories));
            }
            catch (Exception ex)
            {
                // Ghi log lỗi khi lấy danh mục
                _logger.LogError(ex, "Error getting categories");
                return StatusCode(500, ResponseDTO<IEnumerable<CategoryDTO>>.Fail("An error occurred while retrieving categories."));
            }
        }

        // GET: api/Categories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseDTO<CategoryDTO>>> GetCategory(int id)
        {
            try
            {
                // Lấy danh mục theo id
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    return NotFound(ResponseDTO<CategoryDTO>.Fail("Category not found."));
                }
                return Ok(ResponseDTO<CategoryDTO>.Success(category));
            }
            catch (Exception ex)
            {
                // Ghi log lỗi khi lấy danh mục theo id
                _logger.LogError(ex, "Error getting category {Id}", id);
                return StatusCode(500, ResponseDTO<CategoryDTO>.Fail("An error occurred while retrieving the category."));
            }
        }

        // POST: api/Categories
        [HttpPost]
        // Tạm thời bỏ yêu cầu xác thực để test
        // [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<ResponseDTO<CategoryDTO>>> CreateCategory(CreateCategoryDTO categoryDto)
        {
            try
            {
                // Kiểm tra dữ liệu đầu vào hợp lệ
                if (!ModelState.IsValid)
                {
                    return BadRequest(ResponseDTO<CategoryDTO>.Fail("Invalid category data.", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
                }

                // Thêm danh mục mới
                var category = await _categoryService.AddCategoryAsync(categoryDto);
                return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, ResponseDTO<CategoryDTO>.Success(category));
            }
            catch (InvalidOperationException ex)
            {
                // Bắt lỗi nghiệp vụ (ví dụ: trùng tên)
                _logger.LogWarning(ex, "Validation error creating category");
                return BadRequest(ResponseDTO<CategoryDTO>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                // Ghi log lỗi khi thêm danh mục
                _logger.LogError(ex, "Error creating category");
                return StatusCode(500, ResponseDTO<CategoryDTO>.Fail("An error occurred while creating the category."));
            }
        }

        // PUT: api/Categories/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<ResponseDTO<CategoryDTO>>> UpdateCategory(int id, UpdateCategoryDTO categoryDto)
        {
            try
            {
                // Kiểm tra dữ liệu đầu vào hợp lệ
                if (!ModelState.IsValid)
                {
                    return BadRequest(ResponseDTO<CategoryDTO>.Fail("Invalid category data.", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
                }

                // Cập nhật danh mục
                var category = await _categoryService.UpdateCategoryAsync(id, categoryDto);
                if (category == null)
                {
                    return NotFound(ResponseDTO<CategoryDTO>.Fail("Category not found."));
                }

                return Ok(ResponseDTO<CategoryDTO>.Success(category));
            }
            catch (InvalidOperationException ex)
            {
                // Bắt lỗi nghiệp vụ (ví dụ: trùng tên)
                _logger.LogWarning(ex, "Validation error updating category {Id}", id);
                return BadRequest(ResponseDTO<CategoryDTO>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                // Ghi log lỗi khi cập nhật danh mục
                _logger.LogError(ex, "Error updating category {Id}", id);
                return StatusCode(500, ResponseDTO<CategoryDTO>.Fail("An error occurred while updating the category."));
            }
        }

        // DELETE: api/Categories/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<ResponseDTO<bool>>> DeleteCategory(int id)
        {
            try
            {
                // Xóa danh mục theo id
                var result = await _categoryService.DeleteCategoryAsync(id);
                if (!result)
                {
                    return NotFound(ResponseDTO<bool>.Fail("Category not found."));
                }

                return Ok(ResponseDTO<bool>.Success(true, "Category deleted successfully."));
            }
            catch (Exception ex)
            {
                // Ghi log lỗi khi xóa danh mục
                _logger.LogError(ex, "Error deleting category {Id}", id);
                return StatusCode(500, ResponseDTO<bool>.Fail("An error occurred while deleting the category."));
            }
        }
    }
}
