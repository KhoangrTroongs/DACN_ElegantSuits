using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ElegantSuits.Infrastructure.Persistence;

namespace ElegantSuits.Api.Controllers;

public class ToggleVisibilityModel
{
    public int Id { get; set; }
}

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
public class ProductVisibilityController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProductVisibilityController> _logger;

    public ProductVisibilityController(
        ApplicationDbContext context,
        ILogger<ProductVisibilityController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost("toggle")]
    public async Task<IActionResult> ToggleVisibility([FromBody] ToggleVisibilityModel model)
    {
        try
        {
            if (model.Id <= 0)
            {
                return BadRequest(new { success = false, message = "ID sản phẩm không hợp lệ" });
            }

            var product = await _context.Products.FindAsync(model.Id);
            if (product == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy sản phẩm" });
            }

            product.IsHidden = !product.IsHidden;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = product.IsHidden ? "Đã ẩn sản phẩm thành công" : "Đã hiển thị sản phẩm thành công",
                product = new
                {
                    id = product.Id,
                    name = product.Name,
                    isHidden = product.IsHidden
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi thay đổi trạng thái ẩn/hiện sản phẩm ID {Id}", model.Id);
            return StatusCode(500, new { success = false, message = "Có lỗi xảy ra khi cập nhật trạng thái sản phẩm" });
        }
    }
}
