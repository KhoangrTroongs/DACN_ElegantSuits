using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NgoHuuDuc_2280600725.Data;
using NgoHuuDuc_2280600725.Models.Enums;
using NgoHuuDuc_2280600725.Services.Interfaces;

namespace NgoHuuDuc_2280600725.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PaymentApiController : ControllerBase
    {
        private readonly IVnPayService _vnPayService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PaymentApiController> _logger;

        public PaymentApiController(
            IVnPayService vnPayService,
            ApplicationDbContext context,
            ILogger<PaymentApiController> logger)
        {
            _vnPayService = vnPayService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Tạo URL thanh toán VNPay cho đơn hàng
        /// </summary>
        /// <param name="orderId">ID đơn hàng</param>
        /// <returns>URL thanh toán VNPay</returns>
        [HttpPost("vnpay/create/{orderId}")]
        public async Task<IActionResult> CreateVnPayPayment(int orderId)
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
                    Amount = (double)order.TotalAmount,
                    CreatedDate = DateTime.Now,
                    Description = $"Thanh toán đơn hàng #{orderId}",
                    FullName = "Khách hàng POS",
                    OrderId = orderId.ToString()
                };

                var paymentUrl = _vnPayService.CreatePaymentUrl(HttpContext, vnPayModel);

                // Update order status to pending
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
        /// Kiểm tra trạng thái thanh toán của đơn hàng
        /// </summary>
        /// <param name="orderId">ID đơn hàng</param>
        /// <returns>Trạng thái thanh toán</returns>
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
                    totalAmount = order.TotalAmount,
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
        /// Đánh dấu đơn hàng đã thanh toán tiền mặt (POS)
        /// </summary>
        /// <param name="orderId">ID đơn hàng</param>
        /// <returns>Kết quả</returns>
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
}

