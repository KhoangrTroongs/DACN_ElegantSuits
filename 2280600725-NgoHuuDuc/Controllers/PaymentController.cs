using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NgoHuuDuc_2280600725.Data;
using NgoHuuDuc_2280600725.Models.Enums;
using NgoHuuDuc_2280600725.Models.MoMo;
using NgoHuuDuc_2280600725.Services.Interfaces;

namespace NgoHuuDuc_2280600725.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IMoMoService _moMoService;
        private readonly IVnPayService _vnPayService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IMoMoService moMoService,
            IVnPayService vnPayService,
            ApplicationDbContext context,
            ILogger<PaymentController> logger)
        {
            _moMoService = moMoService;
            _vnPayService = vnPayService;
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateMoMoPayment(int orderId)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    return NotFound("Order not found");
                }

                var orderInfo = $"Thanh toán đơn hàng #{orderId}";
                var result = await _moMoService.CreatePaymentAsync(
                    orderId.ToString(),
                    order.TotalAmount,
                    orderInfo);

                if (result.ResultCode == 0)
                {
                    // Save payment info to database
                    order.PaymentMethod = "MoMo";
                    order.PaymentStatus = PaymentStatus.Pending;
                    await _context.SaveChangesAsync();

                    return Json(new { success = true, payUrl = result.PayUrl });
                }
                else
                {
                    _logger.LogError("MoMo payment creation failed: {Message}", result.Message);
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating MoMo payment");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tạo thanh toán" });
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> MoMoReturn([FromQuery] MoMoPaymentResultRequest result)
        {
            try
            {
                _logger.LogInformation("MoMo return callback received: {Result}", System.Text.Json.JsonSerializer.Serialize(result));

                if (!_moMoService.ValidateSignature(result))
                {
                    _logger.LogWarning("Invalid MoMo signature");
                    TempData["ErrorMessage"] = "Chữ ký không hợp lệ";
                    return RedirectToAction("OrderFailed", "Order");
                }

                var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id.ToString() == result.OrderId);
                if (order == null)
                {
                    _logger.LogWarning("Order not found: {OrderId}", result.OrderId);
                    TempData["ErrorMessage"] = "Không tìm thấy đơn hàng";
                    return RedirectToAction("OrderFailed", "Order");
                }

                if (result.ResultCode == 0)
                {
                    // Payment successful
                    order.PaymentStatus = PaymentStatus.Paid;
                    order.OrderStatus = OrderStatus.Confirmed;
                    order.TransactionId = result.TransId.ToString();
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Thanh toán thành công!";
                    return RedirectToAction("OrderSuccess", "Order", new { id = order.Id });
                }
                else
                {
                    // Payment failed
                    order.PaymentStatus = PaymentStatus.Failed;
                    await _context.SaveChangesAsync();

                    TempData["ErrorMessage"] = $"Thanh toán thất bại: {result.Message}";
                    return RedirectToAction("OrderFailed", "Order", new { id = order.Id });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing MoMo return");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xử lý thanh toán";
                return RedirectToAction("OrderFailed", "Order");
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> MoMoNotify([FromBody] MoMoPaymentResultRequest result)
        {
            try
            {
                _logger.LogInformation("MoMo IPN received: {Result}", System.Text.Json.JsonSerializer.Serialize(result));

                if (!_moMoService.ValidateSignature(result))
                {
                    _logger.LogWarning("Invalid MoMo signature in IPN");
                    return Json(new { resultCode = 97, message = "Invalid signature" });
                }

                var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id.ToString() == result.OrderId);
                if (order == null)
                {
                    _logger.LogWarning("Order not found in IPN: {OrderId}", result.OrderId);
                    return Json(new { resultCode = 1, message = "Order not found" });
                }

                if (result.ResultCode == 0)
                {
                    order.PaymentStatus = PaymentStatus.Paid;
                    order.OrderStatus = OrderStatus.Confirmed;
                    order.TransactionId = result.TransId.ToString();
                    await _context.SaveChangesAsync();
                }

                return Json(new { resultCode = 0, message = "Success" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing MoMo IPN");
                return Json(new { resultCode = 99, message = "System error" });
            }
        }
        [HttpPost]
        public async Task<IActionResult> CreateVnPayPayment(int orderId)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    return NotFound("Order not found");
                }

                var vnPayModel = new VnPayPaymentRequestModel
                {
                    Amount = (double)order.TotalAmount,
                    CreatedDate = DateTime.Now,
                    Description = $"Thanh toán đơn hàng #{orderId}",
                    FullName = "Khách hàng", // Có thể lấy từ User.Identity.Name
                    OrderId = orderId.ToString()
                };

                var paymentUrl = _vnPayService.CreatePaymentUrl(HttpContext, vnPayModel);

                // Update order status to pending
                order.PaymentMethod = "VnPay";
                order.PaymentStatus = PaymentStatus.Pending;
                await _context.SaveChangesAsync();

                return Json(new { success = true, payUrl = paymentUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating VnPay payment");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tạo thanh toán VnPay" });
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> VnPayReturn()
        {
            try
            {
                _logger.LogInformation("VnPay return callback received");
                _logger.LogInformation("Query string: {QueryString}", Request.QueryString.Value);
                
                // Check if this is a mobile request
                var isMobile = Request.Query.ContainsKey("mobile") && Request.Query["mobile"] == "true";
                
                var response = _vnPayService.PaymentExecute(Request.Query);
                
                _logger.LogInformation("VnPay response - Success: {Success}, ResponseCode: {ResponseCode}, OrderId: {OrderId}", 
                    response.Success, response.VnPayResponseCode, response.OrderId);

                if (response == null)
                {
                    _logger.LogError("VnPay response is null");
                    if (isMobile)
                    {
                        return Redirect($"elegantsuits://payment-result?orderId=0&status=error");
                    }
                    TempData["ErrorMessage"] = "Không nhận được phản hồi từ VnPay";
                    return RedirectToAction("PaymentFailed");
                }

                if (!response.Success)
                {
                    _logger.LogWarning("VnPay signature validation failed or response unsuccessful");
                    if (isMobile)
                    {
                        return Redirect($"elegantsuits://payment-result?orderId={response.OrderId}&status=failed");
                    }
                    TempData["ErrorMessage"] = "Chữ ký không hợp lệ hoặc giao dịch không thành công";
                    return RedirectToAction("PaymentFailed");
                }

                if (response.VnPayResponseCode == "00")
                {
                    var orderId = response.OrderId;
                    _logger.LogInformation("Processing successful payment for order: {OrderId}", orderId);
                    _logger.LogInformation("OrderId type: {Type}, Value: '{Value}'", orderId?.GetType().Name, orderId);
                    
                    // Try to parse orderId to int
                    if (int.TryParse(orderId, out int orderIdInt))
                    {
                        _logger.LogInformation("Parsed OrderId to int: {OrderIdInt}", orderIdInt);
                        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderIdInt);

                        if (order != null)
                        {
                            order.PaymentStatus = PaymentStatus.Paid;
                            order.OrderStatus = OrderStatus.Confirmed;
                            order.TransactionId = response.TransactionId;
                            await _context.SaveChangesAsync();

                            _logger.LogInformation("Order {OrderId} updated successfully", orderId);
                            
                            // Redirect to mobile app if mobile request
                            if (isMobile)
                            {
                                return Redirect($"elegantsuits://payment-result?orderId={orderId}&status=success");
                            }
                            
                            TempData["SuccessMessage"] = $"Thanh toán VnPay thành công! Mã đơn hàng: #{orderId}";
                            return RedirectToAction("PaymentSuccess");
                        }
                        else
                        {
                            _logger.LogWarning("Order not found in database for OrderId: {OrderId}", orderIdInt);
                            
                            // Log all orders to debug
                            var allOrderIds = await _context.Orders.Select(o => o.Id).ToListAsync();
                            _logger.LogWarning("Available order IDs in database: {OrderIds}", string.Join(", ", allOrderIds));
                            
                            if (isMobile)
                            {
                                return Redirect($"elegantsuits://payment-result?orderId={orderId}&status=notfound");
                            }
                            
                            TempData["ErrorMessage"] = $"Không tìm thấy đơn hàng #{orderId}";
                            return RedirectToAction("PaymentFailed");
                        }
                    }
                    else
                    {
                        _logger.LogError("Failed to parse OrderId '{OrderId}' to int", orderId);
                        if (isMobile)
                        {
                            return Redirect($"elegantsuits://payment-result?orderId=0&status=invalid");
                        }
                        TempData["ErrorMessage"] = "Mã đơn hàng không hợp lệ";
                        return RedirectToAction("PaymentFailed");
                    }
                }

                _logger.LogWarning("VnPay payment failed with response code: {ResponseCode}", response.VnPayResponseCode);
                if (isMobile)
                {
                    return Redirect($"elegantsuits://payment-result?orderId={response.OrderId}&status={response.VnPayResponseCode}");
                }
                TempData["ErrorMessage"] = $"Thanh toán VnPay thất bại. Mã lỗi: {response.VnPayResponseCode}";
                return RedirectToAction("PaymentFailed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing VnPay return");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xử lý thanh toán VnPay: " + ex.Message;
                return RedirectToAction("PaymentFailed");
            }
        }

        [AllowAnonymous]
        public IActionResult PaymentSuccess()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult PaymentFailed()
        {
            return View();
        }
    }
}

