using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NgoHuuDuc_2280600725.DTOs;
using NgoHuuDuc_2280600725.Services.Interfaces;

namespace NgoHuuDuc_2280600725.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoryApiController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryApiController> _logger;

        public CategoryApiController(ICategoryService categoryService, ILogger<CategoryApiController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        // GET: api/CategoryApi
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetCategories()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories");
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách danh mục" });
            }
        }

        // GET: api/CategoryApi/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<CategoryDTO>> GetCategory(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    return NotFound(new { message = "Không tìm thấy danh mục" });
                }
                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category");
                return StatusCode(500, new { message = "Lỗi khi lấy thông tin danh mục" });
            }
        }

        // POST: api/CategoryApi
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<CategoryDTO>> CreateCategory([FromBody] CreateCategoryDTO categoryDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdCategory = await _categoryService.AddCategoryAsync(categoryDto);
                return CreatedAtAction(nameof(GetCategory), new { id = createdCategory.Id }, createdCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return StatusCode(500, new { message = "Lỗi khi tạo danh mục" });
            }
        }

        // PUT: api/CategoryApi/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDTO categoryDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _categoryService.UpdateCategoryAsync(id, categoryDto);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category");
                return StatusCode(500, new { message = "Lỗi khi cập nhật danh mục" });
            }
        }

        // DELETE: api/CategoryApi/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                await _categoryService.DeleteCategoryAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category");
                return StatusCode(500, new { message = "Lỗi khi xóa danh mục" });
            }
        }
    }
}
