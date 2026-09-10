using System.Net.Http.Headers;
using ElegantSuits.Web.Models;

namespace ElegantSuits.Web.Services;

public interface IOrderApiClient
{
    Task<ResponseDTO<List<OrderViewModel>>> GetUserOrdersAsync(string? token = null);
    Task<ResponseDTO<OrderViewModel>> GetOrderByIdAsync(int id, string? token = null);
    Task<ResponseDTO<int>> CreateOrderAsync(CreateOrderRequest request, string? token = null);
    Task<ResponseDTO<bool>> CancelOrderAsync(int id, string? token = null);
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

    public async Task<ResponseDTO<List<OrderViewModel>>> GetUserOrdersAsync(string? token = null)
    {
        try
        {
            AttachToken(token);
            var response = await _httpClient.GetFromJsonAsync<ResponseDTO<List<OrderViewModel>>>("/api/orders");
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
            return await res.Content.ReadFromJsonAsync<ResponseDTO<int>>()
                ?? new ResponseDTO<int> { IsSuccess = false };
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
}
