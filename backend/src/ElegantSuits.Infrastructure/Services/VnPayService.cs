using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ElegantSuits.Infrastructure.Helpers;

namespace ElegantSuits.Infrastructure.Services;

public class VnPayPaymentRequestModel
{
    public string OrderId { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Description { get; set; } = "";
    public double Amount { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
}

public class VnPayPaymentResponseModel
{
    public bool Success { get; set; }
    public string? PaymentMethod { get; set; }
    public string? OrderDescription { get; set; }
    public string? OrderId { get; set; }
    public string? PaymentId { get; set; }
    public string? TransactionId { get; set; }
    public string? Token { get; set; }
    public string? VnPayResponseCode { get; set; }
}

public interface IVnPayService
{
    string CreatePaymentUrl(HttpContext context, VnPayPaymentRequestModel model, string? returnUrl = null);
    VnPayPaymentResponseModel PaymentExecute(IQueryCollection collections);
}

public class VnPayService : IVnPayService
{
    private readonly IConfiguration _config;

    public VnPayService(IConfiguration config)
    {
        _config = config;
    }

    public string CreatePaymentUrl(HttpContext context, VnPayPaymentRequestModel model, string? returnUrl = null)
    {
        var vnpay = new VnPayLibrary();
        
        vnpay.AddRequestData("vnp_Version", _config["VnPay:Version"] ?? "2.1.0");
        vnpay.AddRequestData("vnp_Command", _config["VnPay:Command"] ?? "pay");
        vnpay.AddRequestData("vnp_TmnCode", _config["VnPay:TmnCode"] ?? "MERCHANT_CODE");
        vnpay.AddRequestData("vnp_Amount", ((long)(model.Amount * 100)).ToString());
        
        vnpay.AddRequestData("vnp_CreateDate", model.CreatedDate.ToString("yyyyMMddHHmmss"));
        vnpay.AddRequestData("vnp_CurrCode", _config["VnPay:CurrCode"] ?? "VND");
        vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress(context));
        vnpay.AddRequestData("vnp_Locale", _config["VnPay:Locale"] ?? "vn");
        
        vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang: " + model.OrderId);
        vnpay.AddRequestData("vnp_OrderType", "other");
        
        var finalReturnUrl = returnUrl ?? _config["VnPay:PaymentBackReturnUrl"] ?? "http://localhost:5050/Payment/VnPayReturn";
        vnpay.AddRequestData("vnp_ReturnUrl", finalReturnUrl);
        vnpay.AddRequestData("vnp_TxnRef", model.OrderId);

        var paymentUrl = vnpay.CreateRequestUrl(_config["VnPay:BaseUrl"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html", _config["VnPay:HashSecret"] ?? "SECRET_KEY");

        return paymentUrl;
    }

    public VnPayPaymentResponseModel PaymentExecute(IQueryCollection collections)
    {
        var vnpay = new VnPayLibrary();
        var vnp_SecureHash = collections.FirstOrDefault(p => p.Key == "vnp_SecureHash").Value.ToString();
        
        foreach (var (key, value) in collections)
        {
            if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_") && key != "vnp_SecureHash")
            {
                vnpay.AddResponseData(key, value.ToString());
            }
        }

        var vnp_orderId = vnpay.GetResponseData("vnp_TxnRef");
        var vnp_TransactionId = vnpay.GetResponseData("vnp_TransactionNo");
        var vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
        var vnp_OrderInfo = vnpay.GetResponseData("vnp_OrderInfo");

        var hashSecret = _config["VnPay:HashSecret"] ?? "SECRET_KEY";
        bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, hashSecret);
        
        if (!checkSignature)
        {
            return new VnPayPaymentResponseModel
            {
                Success = false,
                VnPayResponseCode = vnp_ResponseCode
            };
        }

        return new VnPayPaymentResponseModel
        {
            Success = vnp_ResponseCode == "00",
            PaymentMethod = "VnPay",
            OrderDescription = vnp_OrderInfo,
            OrderId = vnp_orderId,
            TransactionId = vnp_TransactionId,
            Token = vnp_SecureHash,
            VnPayResponseCode = vnp_ResponseCode
        };
    }
}
