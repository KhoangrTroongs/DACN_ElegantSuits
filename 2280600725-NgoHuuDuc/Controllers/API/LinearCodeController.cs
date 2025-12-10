using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NgoHuuDuc_2280600725.Data;

namespace NgoHuuDuc_2280600725.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LinearCodeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LinearCodeController> _logger;

        public LinearCodeController(ApplicationDbContext context, ILogger<LinearCodeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Cập nhật mã Linear cho tất cả sản phẩm
        /// </summary>
        [HttpPost("update-all")]
        public async Task<IActionResult> UpdateAllLinearCodes()
        {
            try
            {
                var updates = new Dictionary<int, string>
                {
                    {1, "200000000001"}, {2, "200000000002"}, {3, "200000000003"},
                    {4, "200000000004"}, {5, "200000000005"}, {6, "200000000006"},
                    {7, "200000000007"}, {8, "200000000008"}, {9, "200000000009"},
                    {10, "200000000010"}, {11, "200000000011"}, {12, "200000000012"},
                    {13, "200000000013"}, {14, "200000000014"}, {15, "200000000015"},
                    {16, "200000000016"}, {17, "200000000017"}, {18, "200000000018"},
                    {19, "200000000019"}, {20, "200000000020"}, {21, "200000000021"},
                    {22, "200000000022"}, {23, "200000000023"}, {24, "200000000024"},
                    {25, "200000000025"}, {26, "200000000026"}, {27, "200000000027"},
                    {28, "200000000028"}, {29, "200000000029"}, {30, "200000000030"}
                };

                int updatedCount = 0;
                int notFoundCount = 0;

                foreach (var kvp in updates)
                {
                    var product = await _context.Products.FindAsync(kvp.Key);
                    if (product != null)
                    {
                        product.LinearCode = kvp.Value;
                        updatedCount++;
                        _logger.LogInformation("Updated LinearCode for Product ID {Id}: {LinearCode}", kvp.Key, kvp.Value);
                    }
                    else
                    {
                        notFoundCount++;
                        _logger.LogWarning("Product ID {Id} not found", kvp.Key);
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Đã cập nhật {updatedCount} sản phẩm, {notFoundCount} không tìm thấy",
                    updatedCount,
                    notFoundCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating linear codes");
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Lỗi khi cập nhật mã Linear: {ex.Message}"
                });
            }
        }
    }
}
