using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ElegantSuits.Domain.Enums;
using ElegantSuits.Infrastructure.Persistence;
using ElegantSuits.Infrastructure.Services;

namespace ElegantSuits.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentApiController : ControllerBase
{
    private readonly IVnPayService _vnPayService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PaymentApiController> _logger;
    private readonly IConfiguration _configuration;

    public PaymentApiController(
        IVnPayService vnPayService,
        ApplicationDbContext context,
        ILogger<PaymentApiController> logger,
        IConfiguration configuration)
    {
        _vnPayService = vnPayService;
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Tạo URL thanh toán VNPay cho đơn hàng
    /// </summary>
    [HttpPost("vnpay/create/{orderId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> CreateVnPayPayment(int orderId, [FromQuery] string? clientHost = null)
    {
        try
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy đơn hàng" });
            }

            var vnPayModel = new VnPayPaymentRequestModel
            {
                Amount = (double)order.TotalPrice,
                CreatedDate = DateTime.Now,
                Description = $"Thanh toán đơn hàng #{orderId}",
                FullName = "Khách hàng ElegantSuits",
                OrderId = orderId.ToString()
            };

            _logger.LogInformation("Creating VNPay URL for OrderId: {OrderId}", orderId);

            string? returnUrl = null;
            if (!string.IsNullOrEmpty(clientHost))
            {
                returnUrl = $"http://{clientHost}:5050/Payment/VnPayReturn";
            }
            
            var paymentUrl = _vnPayService.CreatePaymentUrl(HttpContext, vnPayModel, returnUrl);

            order.PaymentMethod = "VnPay";
            order.PaymentStatus = PaymentStatus.Pending;
            await _context.SaveChangesAsync();

            return Ok(new { 
                success = true, 
                payUrl = paymentUrl,
                orderId = orderId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating VnPay payment for order {OrderId}", orderId);
            return StatusCode(500, new { 
                success = false, 
                message = "Có lỗi xảy ra khi tạo thanh toán VnPay" 
            });
        }
    }

    /// <summary>
    /// Xử lý callback / return từ VNPay
    /// </summary>
    [HttpGet("vnpay/callback")]
    public async Task<IActionResult> VnPayCallback()
    {
        try
        {
            var response = _vnPayService.PaymentExecute(Request.Query);
            if (response.Success && int.TryParse(response.OrderId, out int orderId))
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order != null)
                {
                    order.PaymentStatus = PaymentStatus.Paid;
                    order.OrderStatus = OrderStatus.Confirmed;
                    await _context.SaveChangesAsync();
                }
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing VnPay callback");
            return StatusCode(500, new { success = false, message = "Lỗi xử lý kết quả VNPay" });
        }
    }

    /// <summary>
    /// Kiểm tra trạng thái thanh toán của đơn hàng
    /// </summary>
    [HttpGet("status/{orderId}")]
    public async Task<IActionResult> GetPaymentStatus(int orderId)
    {
        try
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy đơn hàng" });
            }

            return Ok(new {
                success = true,
                orderId = order.Id,
                paymentStatus = order.PaymentStatus.ToString(),
                paymentMethod = order.PaymentMethod,
                totalAmount = order.TotalPrice,
                isPaid = order.PaymentStatus == PaymentStatus.Paid
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment status for order {OrderId}", orderId);
            return StatusCode(500, new { 
                success = false, 
                message = "Có lỗi xảy ra khi kiểm tra trạng thái thanh toán" 
            });
        }
    }

    /// <summary>
    /// Đánh dấu đơn hàng đã thanh toán tiền mặt (POS/Admin)
    /// </summary>
    [HttpPost("cash/{orderId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
    public async Task<IActionResult> MarkAsCashPayment(int orderId)
    {
        try
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy đơn hàng" });
            }

            order.PaymentMethod = "Cash";
            order.PaymentStatus = PaymentStatus.Paid;
            order.OrderStatus = OrderStatus.Confirmed;
            await _context.SaveChangesAsync();

            return Ok(new {
                success = true,
                message = "Đã xác nhận thanh toán tiền mặt",
                orderId = order.Id
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking cash payment for order {OrderId}", orderId);
            return StatusCode(500, new { 
                success = false, 
                message = "Có lỗi xảy ra khi xác nhận thanh toán" 
            });
        }
    }
}
