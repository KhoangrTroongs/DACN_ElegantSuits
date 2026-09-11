using System.Net.Http.Headers;
using System.Text.Json;
using ElegantSuits.Web.Models;

namespace ElegantSuits.Web.Services;

public interface IOrderApiClient
{
    Task<ResponseDTO<List<OrderViewModel>>> GetAllOrdersAsync(string? token = null);
    Task<ResponseDTO<List<OrderViewModel>>> GetUserOrdersAsync(string? token = null);
    Task<ResponseDTO<OrderViewModel>> GetOrderByIdAsync(int id, string? token = null);
    Task<ResponseDTO<int>> CreateOrderAsync(CreateOrderRequest request, string? token = null);
    Task<ResponseDTO<bool>> CancelOrderAsync(int id, string? token = null);
    Task<ResponseDTO<bool>> UpdateOrderStatusAsync(int id, int status, string? token = null);
}

public class OrderApiClient : IOrderApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OrderApiClient> _logger;

    public OrderApiClient(HttpClient httpClient, ILogger<OrderApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    private void AttachToken(string? token)
    {
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<ResponseDTO<List<OrderViewModel>>> GetAllOrdersAsync(string? token = null)
    {
        try
        {
            AttachToken(token);
            var response = await _httpClient.GetFromJsonAsync<ResponseDTO<List<OrderViewModel>>>("/api/orders");
            return response ?? new ResponseDTO<List<OrderViewModel>> { IsSuccess = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all orders for admin");
            return new ResponseDTO<List<OrderViewModel>> { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ResponseDTO<List<OrderViewModel>>> GetUserOrdersAsync(string? token = null)
    {
        try
        {
            AttachToken(token);
            var response = await _httpClient.GetFromJsonAsync<ResponseDTO<List<OrderViewModel>>>("/api/orders/my-orders");
            return response ?? new ResponseDTO<List<OrderViewModel>> { IsSuccess = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user orders");
            return new ResponseDTO<List<OrderViewModel>> { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ResponseDTO<OrderViewModel>> GetOrderByIdAsync(int id, string? token = null)
    {
        try
        {
            AttachToken(token);
            var response = await _httpClient.GetFromJsonAsync<ResponseDTO<OrderViewModel>>($"/api/orders/{id}");
            return response ?? new ResponseDTO<OrderViewModel> { IsSuccess = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order {Id}", id);
            return new ResponseDTO<OrderViewModel> { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ResponseDTO<int>> CreateOrderAsync(CreateOrderRequest request, string? token = null)
    {
        try
        {
            AttachToken(token);
            var res = await _httpClient.PostAsJsonAsync("/api/orders", request);
            var json = await res.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json))
            {
                return new ResponseDTO<int> { IsSuccess = res.IsSuccessStatusCode, Message = res.IsSuccessStatusCode ? null : "Không nhận được phản hồi từ máy chủ." };
            }

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            bool isSuccess = false;
            if (root.TryGetProperty("isSuccess", out var s1) || root.TryGetProperty("IsSuccess", out s1))
            {
                isSuccess = s1.GetBoolean();
            }
            else
            {
                isSuccess = res.IsSuccessStatusCode;
            }

            string? message = null;
            if (root.TryGetProperty("message", out var m1) || root.TryGetProperty("Message", out m1))
            {
                if (m1.ValueKind == JsonValueKind.String)
                {
                    message = m1.GetString();
                }
            }

            int orderId = 0;
            if (root.TryGetProperty("data", out var d1) || root.TryGetProperty("Data", out d1))
            {
                if (d1.ValueKind == JsonValueKind.Number)
                {
                    orderId = d1.GetInt32();
                }
                else if (d1.ValueKind == JsonValueKind.Object)
                {
                    if (d1.TryGetProperty("id", out var idProp) || d1.TryGetProperty("Id", out idProp))
                    {
                        if (idProp.ValueKind == JsonValueKind.Number)
                        {
                            orderId = idProp.GetInt32();
                        }
                    }
                }
            }

            return new ResponseDTO<int>
            {
                IsSuccess = isSuccess,
                Message = message,
                Data = orderId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return new ResponseDTO<int> { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ResponseDTO<bool>> CancelOrderAsync(int id, string? token = null)
    {
        try
        {
            AttachToken(token);
            var res = await _httpClient.PostAsync($"/api/orders/{id}/cancel", null);
            return await res.Content.ReadFromJsonAsync<ResponseDTO<bool>>()
                ?? new ResponseDTO<bool> { IsSuccess = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling order {Id}", id);
            return new ResponseDTO<bool> { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ResponseDTO<bool>> UpdateOrderStatusAsync(int id, int status, string? token = null)
    {
        try
        {
            AttachToken(token);
            var res = await _httpClient.PutAsJsonAsync($"/api/orders/{id}/status", new { Status = status });
            return await res.Content.ReadFromJsonAsync<ResponseDTO<bool>>()
                ?? new ResponseDTO<bool> { IsSuccess = res.IsSuccessStatusCode };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status for order {Id}", id);
            return new ResponseDTO<bool> { IsSuccess = false, Message = ex.Message };
        }
    }
}
