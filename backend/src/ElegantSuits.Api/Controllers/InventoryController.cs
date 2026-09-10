using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ElegantSuits.Application.Common.Models;
using ElegantSuits.Infrastructure.Persistence;

namespace ElegantSuits.Api.Controllers;

public class InventoryItemDTO
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public string CategoryName { get; set; } = "";
    public int Quantity { get; set; }
    public string? LinearCode { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
}

public class UpdateQuantityDTO
{
    public int Quantity { get; set; }
    public bool IsAbsolute { get; set; } = false;
}

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class InventoryController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<InventoryController> _logger;

    public InventoryController(ApplicationDbContext context, ILogger<InventoryController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/Inventory
    [HttpGet]
    public async Task<ActionResult<ResponseDTO<IEnumerable<InventoryItemDTO>>>> GetInventory()
    {
        try
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .OrderBy(p => p.Name)
                .Select(p => new InventoryItemDTO
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    CategoryName = p.Category != null ? p.Category.Name : "",
                    Quantity = p.Quantity,
                    LinearCode = p.LinearCode,
                    ImageUrl = p.ImageUrl,
                    Price = p.Price
                })
                .ToListAsync();

            return Ok(ResponseDTO<IEnumerable<InventoryItemDTO>>.Success(products));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventory");
            return StatusCode(500, ResponseDTO<IEnumerable<InventoryItemDTO>>.Fail("Lỗi khi lấy danh sách tồn kho"));
        }
    }

    // GET: api/Inventory/by-linear/{linearCode}
    [HttpGet("by-linear/{linearCode}")]
    public async Task<ActionResult<ResponseDTO<InventoryItemDTO>>> GetByLinearCode(string linearCode)
    {
        try
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.LinearCode == linearCode);

            if (product == null)
            {
                return NotFound(ResponseDTO<InventoryItemDTO>.Fail("Không tìm thấy sản phẩm với mã này"));
            }

            var item = new InventoryItemDTO
            {
                ProductId = product.Id,
                ProductName = product.Name,
                CategoryName = product.Category?.Name ?? "",
                Quantity = product.Quantity,
                LinearCode = product.LinearCode,
                ImageUrl = product.ImageUrl,
                Price = product.Price
            };

            return Ok(ResponseDTO<InventoryItemDTO>.Success(item));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding product by linear code {LinearCode}", linearCode);
            return StatusCode(500, ResponseDTO<InventoryItemDTO>.Fail("Lỗi khi tìm sản phẩm"));
        }
    }

    // PUT: api/Inventory/{productId}/update-quantity
    [HttpPut("{productId}/update-quantity")]
    public async Task<ActionResult<ResponseDTO<InventoryItemDTO>>> UpdateQuantity(int productId, [FromBody] UpdateQuantityDTO dto)
    {
        try
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                return NotFound(ResponseDTO<InventoryItemDTO>.Fail("Không tìm thấy sản phẩm"));
            }

            if (dto.IsAbsolute)
            {
                product.Quantity = dto.Quantity;
            }
            else
            {
                product.Quantity += dto.Quantity;
                if (product.Quantity < 0) product.Quantity = 0;
            }

            await _context.SaveChangesAsync();

            var item = new InventoryItemDTO
            {
                ProductId = product.Id,
                ProductName = product.Name,
                CategoryName = product.Category?.Name ?? "",
                Quantity = product.Quantity,
                LinearCode = product.LinearCode,
                ImageUrl = product.ImageUrl,
                Price = product.Price
            };

            return Ok(ResponseDTO<InventoryItemDTO>.Success(item,
                dto.IsAbsolute ? "Đã cập nhật số lượng" : (dto.Quantity >= 0 ? "Đã nhập kho" : "Đã xuất kho")));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating quantity for product {ProductId}", productId);
            return StatusCode(500, ResponseDTO<InventoryItemDTO>.Fail("Lỗi khi cập nhật số lượng"));
        }
    }

    // POST: api/Inventory/generate-linear-codes
    [HttpPost("generate-linear-codes")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<ResponseDTO<int>>> GenerateLinearCodes()
    {
        try
        {
            var productsWithoutCode = await _context.Products
                .Where(p => string.IsNullOrEmpty(p.LinearCode))
                .ToListAsync();

            if (!productsWithoutCode.Any())
            {
                return Ok(ResponseDTO<int>.Success(0, "Tất cả sản phẩm đã có mã linear"));
            }

            var existingCodes = await _context.Products
                .Where(p => !string.IsNullOrEmpty(p.LinearCode))
                .Select(p => p.LinearCode)
                .ToListAsync();

            var random = new Random();
            var generatedCodes = new HashSet<string>(existingCodes!);
            int count = 0;

            foreach (var product in productsWithoutCode)
            {
                string newCode;
                do
                {
                    newCode = string.Concat(Enumerable.Range(0, 12).Select(_ => random.Next(0, 10).ToString()));
                } while (generatedCodes.Contains(newCode));

                product.LinearCode = newCode;
                generatedCodes.Add(newCode);
                count++;
            }

            await _context.SaveChangesAsync();
            return Ok(ResponseDTO<int>.Success(count, $"Đã tạo mã linear cho {count} sản phẩm"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating linear codes");
            return StatusCode(500, ResponseDTO<int>.Fail("Lỗi khi tạo mã linear"));
        }
    }

    // GET: api/Inventory/export-linear-codes
    [HttpGet("export-linear-codes")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> ExportLinearCodes()
    {
        try
        {
            var products = await _context.Products
                .OrderBy(p => p.Id)
                .Select(p => new { p.Id, p.Name, p.LinearCode })
                .ToListAsync();

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Ma San Pham,Ten San Pham,Ma Linear");

            foreach (var p in products)
            {
                csv.AppendLine($"{p.Id},\"{p.Name.Replace("\"", "\"\"")}\",{p.LinearCode ?? ""}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"LinearCodes_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting linear codes");
            return StatusCode(500, "Lỗi khi xuất file");
        }
    }
}
