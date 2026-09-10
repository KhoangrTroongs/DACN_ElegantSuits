using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using ElegantSuits.Infrastructure.Persistence;

namespace ElegantSuits.Api.Controllers;

public class UpdateStockModel
{
    public int[] Ids { get; set; } = Array.Empty<int>();
    public int Quantity { get; set; }
}

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
public class BulkProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<BulkProductsController> _logger;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public BulkProductsController(
        ApplicationDbContext context,
        ILogger<BulkProductsController> logger,
        IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _logger = logger;
        _webHostEnvironment = webHostEnvironment;
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    // POST: api/BulkProducts/delete
    [HttpPost("delete")]
    public async Task<IActionResult> DeleteBulk([FromBody] int[] ids)
    {
        try
        {
            if (ids == null || ids.Length == 0)
            {
                return BadRequest(new { success = false, message = "Không có sản phẩm nào được chọn" });
            }

            int deletedCount = 0;
            List<string> errors = new List<string>();

            foreach (var id in ids)
            {
                try
                {
                    var product = await _context.Products.FindAsync(id);
                    if (product != null)
                    {
                        if (!string.IsNullOrEmpty(product.ImageUrl))
                        {
                            var imagePath = Path.Combine(_webHostEnvironment.WebRootPath ?? "wwwroot", product.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(imagePath))
                            {
                                System.IO.File.Delete(imagePath);
                            }
                        }

                        _context.Products.Remove(product);
                        deletedCount++;
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Lỗi khi xóa sản phẩm ID {id}: {ex.Message}");
                    _logger.LogError(ex, "Lỗi khi xóa sản phẩm ID {Id}", id);
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = $"Đã xóa {deletedCount} sản phẩm thành công",
                errors = errors
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa hàng loạt sản phẩm");
            return StatusCode(500, new { success = false, message = "Có lỗi xảy ra khi xóa sản phẩm" });
        }
    }

    // POST: api/BulkProducts/updateStock
    [HttpPost("updateStock")]
    public async Task<IActionResult> UpdateStock([FromBody] UpdateStockModel model)
    {
        try
        {
            if (model.Ids == null || model.Ids.Length == 0)
            {
                return BadRequest(new { success = false, message = "Không có sản phẩm nào được chọn" });
            }

            if (model.Quantity < 0)
            {
                return BadRequest(new { success = false, message = "Số lượng không được nhỏ hơn 0" });
            }

            int updatedCount = 0;
            List<string> errors = new List<string>();

            foreach (var id in model.Ids)
            {
                try
                {
                    var product = await _context.Products.FindAsync(id);
                    if (product != null)
                    {
                        product.Quantity = model.Quantity;
                        _context.Products.Update(product);
                        updatedCount++;
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Lỗi khi cập nhật sản phẩm ID {id}: {ex.Message}");
                    _logger.LogError(ex, "Lỗi khi cập nhật tồn kho sản phẩm ID {Id}", id);
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = $"Đã cập nhật tồn kho cho {updatedCount} sản phẩm thành công",
                errors = errors
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật tồn kho hàng loạt");
            return StatusCode(500, new { success = false, message = "Có lỗi xảy ra khi cập nhật tồn kho" });
        }
    }

    // GET: api/BulkProducts/export
    [HttpGet("export")]
    public async Task<IActionResult> ExportToExcel([FromQuery] int[] ids)
    {
        try
        {
            var query = _context.Products.Include(p => p.Category).AsQueryable();
            if (ids != null && ids.Length > 0)
            {
                query = query.Where(p => ids.Contains(p.Id));
            }

            var products = await query.ToListAsync();
            if (!products.Any())
            {
                return NotFound(new { success = false, message = "Không tìm thấy sản phẩm nào" });
            }

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("DanhSachSanPham");

                // Headers
                string[] headers = { "ID", "Tên sản phẩm", "Danh mục", "Giá bán (VNĐ)", "Số lượng", "Mã Barcode", "Trạng thái" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                    worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightSlateGray);
                    worksheet.Cells[1, i + 1].Style.Font.Color.SetColor(Color.White);
                }

                // Data
                int row = 2;
                foreach (var p in products)
                {
                    worksheet.Cells[row, 1].Value = p.Id;
                    worksheet.Cells[row, 2].Value = p.Name;
                    worksheet.Cells[row, 3].Value = p.Category?.Name ?? "N/A";
                    worksheet.Cells[row, 4].Value = p.Price;
                    worksheet.Cells[row, 4].Style.Numberformat.Format = "#,##0";
                    worksheet.Cells[row, 5].Value = p.Quantity;
                    worksheet.Cells[row, 6].Value = p.LinearCode ?? "";
                    worksheet.Cells[row, 7].Value = p.IsHidden ? "Đã ẩn" : "Hiển thị";
                    row++;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                var fileBytes = package.GetAsByteArray();
                var fileName = $"Products_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xuất danh sách sản phẩm ra Excel");
            return StatusCode(500, new { success = false, message = "Lỗi khi xuất file Excel" });
        }
    }
}
