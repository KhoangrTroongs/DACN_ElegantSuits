using Microsoft.AspNetCore.Mvc;
using ElegantSuits.Web.Services;

namespace ElegantSuits.Web.Controllers;

public class PaymentController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        HttpClient httpClient,
        IConfiguration config,
        ILogger<PaymentController> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
    }

    public async Task<IActionResult> CreateVnPayPayment(int orderId)
    {
        try
        {
            var baseUrl = _config["BackendApi:BaseUrl"] ?? "http://localhost:5097";
            var token = HttpContext.Session.GetString("JwtToken");
            
            var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/api/PaymentApi/vnpay/create/{orderId}");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var res = await _httpClient.SendAsync(request);
            if (res.IsSuccessStatusCode)
            {
                var content = await res.Content.ReadFromJsonAsync<VnPayCreateResponse>();
                if (content != null && !string.IsNullOrEmpty(content.PayUrl))
                {
                    return Redirect(content.PayUrl);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating VNPay payment for order {OrderId}", orderId);
        }

        return RedirectToAction(nameof(PaymentFailed), new { orderId, message = "Lỗi kết nối tới cổng thanh toán VNPay." });
    }

    public class VnPayCreateResponse
    {
        public bool Success { get; set; }
        public string? PayUrl { get; set; }
        public int OrderId { get; set; }
    }

    public async Task<IActionResult> VnPayReturn()
    {
        var queryString = Request.QueryString.Value;
        var baseUrl = _config["BackendApi:BaseUrl"] ?? "http://localhost:5097";

        try
        {
            var res = await _httpClient.GetAsync($"{baseUrl}/api/PaymentApi/vnpay/callback{queryString}");
            if (res.IsSuccessStatusCode)
            {
                var vnpResponseCode = Request.Query["vnp_ResponseCode"].ToString();
                var orderIdStr = Request.Query["vnp_TxnRef"].ToString();
                int.TryParse(orderIdStr, out int orderId);

                if (vnpResponseCode == "00")
                {
                    return RedirectToAction(nameof(PaymentSuccess), new { orderId });
                }
                return RedirectToAction(nameof(PaymentFailed), new { orderId, message = $"Giao dịch không thành công. Mã lỗi: {vnpResponseCode}" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling VNPay return");
        }

        return RedirectToAction(nameof(PaymentFailed), new { message = "Lỗi xử lý phản hồi từ VNPay" });
    }

    public IActionResult PaymentSuccess(int orderId)
    {
        ViewBag.OrderId = orderId;
        return View();
    }

    public IActionResult PaymentFailed(int orderId, string? message = null)
    {
        ViewBag.OrderId = orderId;
        ViewBag.Message = message ?? "Thanh toán thất bại";
        return View();
    }
}
