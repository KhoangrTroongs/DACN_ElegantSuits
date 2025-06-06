using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NgoHuuDuc_2280600725.Data;
using NgoHuuDuc_2280600725.Models;
using NgoHuuDuc_2280600725.Models.Enums;
using NgoHuuDuc_2280600725.DTOs;

namespace NgoHuuDuc_2280600725.Controllers.API
{
    /// <summary>
    /// API Controller for statistics and analytics
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrator")]
    public class StatisticsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<StatisticsApiController> _logger;

        public StatisticsApiController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<StatisticsApiController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Lấy thống kê tổng quan của hệ thống
        /// </summary>
        /// <param name="startDate">Ngày bắt đầu (định dạng: yyyy-MM-dd)</param>
        /// <param name="endDate">Ngày kết thúc (định dạng: yyyy-MM-dd)</param>
        /// <returns>Thống kê tổng quan bao gồm doanh thu, đơn hàng, sản phẩm, người dùng</returns>
        /// <response code="200">Trả về thống kê thành công</response>
        /// <response code="400">Tham số không hợp lệ</response>
        /// <response code="401">Không có quyền truy cập</response>
        [HttpGet("overview")]
        [ProducesResponseType(typeof(StatisticsOverviewDTO), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<StatisticsOverviewDTO>> GetOverviewStatistics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                // Mặc định là thống kê tháng hiện tại
                if (!startDate.HasValue)
                {
                    startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                }

                if (!endDate.HasValue)
                {
                    endDate = DateTime.Now;
                }

                // Đảm bảo endDate là cuối ngày
                endDate = endDate.Value.Date.AddDays(1).AddTicks(-1);

                // Validate date range
                if (startDate > endDate)
                {
                    return BadRequest("Ngày bắt đầu không thể lớn hơn ngày kết thúc");
                }

                // Lấy tất cả đơn hàng trong khoảng thời gian
                var orders = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                    .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                    .ToListAsync();

                var statistics = new StatisticsOverviewDTO
                {
                    DateRange = new DateRangeDTO
                    {
                        StartDate = startDate.Value,
                        EndDate = endDate.Value
                    },
                    TotalOrders = orders.Count,
                    TotalRevenue = orders.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.TotalPrice),
                    TotalProducts = await _context.Products.CountAsync(),
                    TotalUsers = await _userManager.Users.CountAsync(),
                    OrdersByStatus = new OrdersByStatusDTO
                    {
                        Pending = orders.Count(o => o.Status == OrderStatus.Pending),
                        Confirmed = orders.Count(o => o.Status == OrderStatus.Confirmed),
                        Shipping = orders.Count(o => o.Status == OrderStatus.Shipping),
                        Delivered = orders.Count(o => o.Status == OrderStatus.Delivered),
                        Cancelled = orders.Count(o => o.Status == OrderStatus.Cancelled),
                        Returned = orders.Count(o => o.Status == OrderStatus.Returned)
                    },
                    RevenueByStatus = new RevenueByStatusDTO
                    {
                        Pending = orders.Where(o => o.Status == OrderStatus.Pending).Sum(o => o.TotalPrice),
                        Confirmed = orders.Where(o => o.Status == OrderStatus.Confirmed).Sum(o => o.TotalPrice),
                        Shipping = orders.Where(o => o.Status == OrderStatus.Shipping).Sum(o => o.TotalPrice),
                        Delivered = orders.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.TotalPrice),
                        Cancelled = orders.Where(o => o.Status == OrderStatus.Cancelled).Sum(o => o.TotalPrice),
                        Returned = orders.Where(o => o.Status == OrderStatus.Returned).Sum(o => o.TotalPrice)
                    }
                };

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê tổng quan");
                return StatusCode(500, "Có lỗi xảy ra khi lấy thống kê");
            }
        }

        /// <summary>
        /// Lấy thống kê doanh thu theo ngày
        /// </summary>
        /// <param name="startDate">Ngày bắt đầu</param>
        /// <param name="endDate">Ngày kết thúc</param>
        /// <returns>Danh sách doanh thu theo từng ngày</returns>
        [HttpGet("daily-revenue")]
        [ProducesResponseType(typeof(List<DailyRevenueDTO>), 200)]
        public async Task<ActionResult<List<DailyRevenueDTO>>> GetDailyRevenue(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                if (!startDate.HasValue)
                {
                    startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                }

                if (!endDate.HasValue)
                {
                    endDate = DateTime.Now;
                }

                endDate = endDate.Value.Date.AddDays(1).AddTicks(-1);

                var orders = await _context.Orders
                    .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                    .ToListAsync();

                var dailyStats = orders
                    .GroupBy(o => o.OrderDate.Date)
                    .Select(g => new DailyRevenueDTO
                    {
                        Date = g.Key,
                        OrderCount = g.Count(),
                        Revenue = g.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.TotalPrice)
                    })
                    .OrderBy(d => d.Date)
                    .ToList();

                return Ok(dailyStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê doanh thu theo ngày");
                return StatusCode(500, "Có lỗi xảy ra khi lấy thống kê doanh thu theo ngày");
            }
        }

        /// <summary>
        /// Lấy top sản phẩm bán chạy
        /// </summary>
        /// <param name="limit">Số lượng sản phẩm muốn lấy (mặc định: 10)</param>
        /// <param name="startDate">Ngày bắt đầu</param>
        /// <param name="endDate">Ngày kết thúc</param>
        /// <returns>Danh sách top sản phẩm bán chạy</returns>
        [HttpGet("top-products")]
        [ProducesResponseType(typeof(TopProductsResponseDTO), 200)]
        public async Task<ActionResult<TopProductsResponseDTO>> GetTopProducts(
            [FromQuery] int limit = 10,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                if (limit <= 0 || limit > 100)
                {
                    return BadRequest("Limit phải từ 1 đến 100");
                }

                if (!startDate.HasValue)
                {
                    startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                }

                if (!endDate.HasValue)
                {
                    endDate = DateTime.Now;
                }

                endDate = endDate.Value.Date.AddDays(1).AddTicks(-1);

                var orders = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                    .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate && o.Status == OrderStatus.Delivered)
                    .ToListAsync();

                var allOrderDetails = orders.SelectMany(o => o.OrderDetails).ToList();
                var totalRevenue = allOrderDetails.Sum(od => od.Price * od.Quantity);

                var topProducts = allOrderDetails
                    .GroupBy(od => new { od.ProductId, od.Product!.Name, od.Product.ImageUrl })
                    .Select(g => new TopProductDTO
                    {
                        ProductId = g.Key.ProductId,
                        ProductName = g.Key.Name,
                        ImageUrl = g.Key.ImageUrl,
                        QuantitySold = g.Sum(od => od.Quantity),
                        Revenue = g.Sum(od => od.Price * od.Quantity)
                    })
                    .OrderByDescending(p => p.QuantitySold)
                    .Take(limit)
                    .ToList();

                // Tính phần trăm và thứ hạng
                for (int i = 0; i < topProducts.Count; i++)
                {
                    topProducts[i].Rank = i + 1;
                    topProducts[i].RevenuePercentage = totalRevenue > 0 ? (topProducts[i].Revenue / totalRevenue) * 100 : 0;
                }

                var response = new TopProductsResponseDTO
                {
                    Products = topProducts,
                    TotalRevenue = totalRevenue,
                    TotalQuantitySold = allOrderDetails.Sum(od => od.Quantity),
                    DateRange = new DateRangeDTO
                    {
                        StartDate = startDate.Value,
                        EndDate = endDate.Value
                    },
                    RequestedLimit = limit
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy top sản phẩm bán chạy");
                return StatusCode(500, "Có lỗi xảy ra khi lấy top sản phẩm bán chạy");
            }
        }
    }
}
