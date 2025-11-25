using NgoHuuDuc_2280600725.Models.MoMo;
using NgoHuuDuc_2280600725.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace NgoHuuDuc_2280600725.Services
{
    public class MoMoService : IMoMoService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<MoMoService> _logger;

        public MoMoService(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<MoMoService> logger)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<MoMoPaymentResponse> CreatePaymentAsync(string orderId, decimal amount, string orderInfo)
        {
            try
            {
                var partnerCode = _configuration["MoMo:PartnerCode"];
                var accessKey = _configuration["MoMo:AccessKey"];
                var secretKey = _configuration["MoMo:SecretKey"];
                var endpoint = _configuration["MoMo:Endpoint"];
                var returnUrl = _configuration["MoMo:ReturnUrl"];
                var notifyUrl = _configuration["MoMo:NotifyUrl"];

                var requestId = Guid.NewGuid().ToString();
                var extraData = "";
                var requestType = "captureWallet";
                var amountLong = (long)amount;

                // Create signature
                var rawSignature = $"accessKey={accessKey}&amount={amountLong}&extraData={extraData}&ipnUrl={notifyUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={returnUrl}&requestId={requestId}&requestType={requestType}";
                var signature = ComputeHmacSha256(rawSignature, secretKey);

                var request = new MoMoPaymentRequest
                {
                    PartnerCode = partnerCode,
                    PartnerName = "Test",
                    StoreId = partnerCode,
                    RequestId = requestId,
                    Amount = amountLong,
                    OrderId = orderId,
                    OrderInfo = orderInfo,
                    RedirectUrl = returnUrl,
                    IpnUrl = notifyUrl,
                    RequestType = requestType,
                    ExtraData = extraData,
                    Lang = "vi",
                    Signature = signature
                };

                var httpClient = _httpClientFactory.CreateClient();
                var jsonContent = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogInformation("Sending MoMo payment request: {Request}", jsonContent);

                var response = await httpClient.PostAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("MoMo payment response: {Response}", responseContent);

                var result = JsonSerializer.Deserialize<MoMoPaymentResponse>(responseContent);
                return result ?? new MoMoPaymentResponse { ResultCode = -1, Message = "Failed to parse response" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating MoMo payment");
                return new MoMoPaymentResponse { ResultCode = -1, Message = ex.Message };
            }
        }

        public bool ValidateSignature(MoMoPaymentResultRequest result)
        {
            try
            {
                var secretKey = _configuration["MoMo:SecretKey"];
                var accessKey = _configuration["MoMo:AccessKey"];

                var rawSignature = $"accessKey={accessKey}&amount={result.Amount}&extraData={result.ExtraData}&message={result.Message}&orderId={result.OrderId}&orderInfo={result.OrderInfo}&orderType={result.OrderType}&partnerCode={result.PartnerCode}&payType={result.PayType}&requestId={result.RequestId}&responseTime={result.ResponseTime}&resultCode={result.ResultCode}&transId={result.TransId}";
                var signature = ComputeHmacSha256(rawSignature, secretKey);

                _logger.LogInformation("Validating signature. Expected: {Expected}, Received: {Received}", signature, result.Signature);

                return signature.Equals(result.Signature, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating MoMo signature");
                return false;
            }
        }

        private string ComputeHmacSha256(string message, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            using var hmac = new HMACSHA256(keyBytes);
            var hashBytes = hmac.ComputeHash(messageBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}

