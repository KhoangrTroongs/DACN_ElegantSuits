using System.Net.Http.Headers;
using ElegantSuits.Web.Models;

namespace ElegantSuits.Web.Services;

public interface ICartApiClient
{
    Task<ResponseDTO<CartViewModel>> GetCartAsync(string? token = null);
    Task<ResponseDTO<CartViewModel>> AddToCartAsync(AddToCartRequest request, string? token = null);
    Task<ResponseDTO<CartViewModel>> UpdateCartItemAsync(int cartItemId, int quantity, string? token = null);
    Task<ResponseDTO<CartViewModel>> RemoveCartItemAsync(int cartItemId, string? token = null);
    Task<ResponseDTO<bool>> ClearCartAsync(string? token = null);
}

public class CartApiClient : ICartApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CartApiClient> _logger;

    public CartApiClient(HttpClient httpClient, ILogger<CartApiClient> logger)
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

    public async Task<ResponseDTO<CartViewModel>> GetCartAsync(string? token = null)
    {
        try
        {
            AttachToken(token);
            var response = await _httpClient.GetFromJsonAsync<ResponseDTO<CartViewModel>>("/api/cart");
            return response ?? new ResponseDTO<CartViewModel> { IsSuccess = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cart");
            return new ResponseDTO<CartViewModel> { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ResponseDTO<CartViewModel>> AddToCartAsync(AddToCartRequest request, string? token = null)
    {
        try
        {
            AttachToken(token);
            var res = await _httpClient.PostAsJsonAsync("/api/cart", request);
            return await res.Content.ReadFromJsonAsync<ResponseDTO<CartViewModel>>()
                ?? new ResponseDTO<CartViewModel> { IsSuccess = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding to cart");
            return new ResponseDTO<CartViewModel> { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ResponseDTO<CartViewModel>> UpdateCartItemAsync(int cartItemId, int quantity, string? token = null)
    {
        try
        {
            AttachToken(token);
            var res = await _httpClient.PutAsJsonAsync("/api/cart", new { cartItemId, quantity });
            if (!res.IsSuccessStatusCode)
            {
                res = await _httpClient.PutAsJsonAsync($"/api/cart/item/{cartItemId}", new { cartItemId, quantity });
            }
            if (!res.IsSuccessStatusCode)
            {
                res = await _httpClient.PutAsJsonAsync($"/api/cart/items/{cartItemId}", new { cartItemId, quantity });
            }

            if (res.IsSuccessStatusCode)
            {
                var data = await res.Content.ReadFromJsonAsync<ResponseDTO<CartViewModel>>();
                return data ?? new ResponseDTO<CartViewModel> { IsSuccess = true, Message = "Đã cập nhật số lượng thành công!" };
            }

            return new ResponseDTO<CartViewModel> { IsSuccess = false, Message = $"Lỗi cập nhật: {res.StatusCode}" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cart item");
            return new ResponseDTO<CartViewModel> { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ResponseDTO<CartViewModel>> RemoveCartItemAsync(int cartItemId, string? token = null)
    {
        try
        {
            AttachToken(token);
            var res = await _httpClient.DeleteAsync($"/api/cart/item/{cartItemId}");
            if (!res.IsSuccessStatusCode)
            {
                res = await _httpClient.DeleteAsync($"/api/cart/items/{cartItemId}");
            }

            if (res.IsSuccessStatusCode)
            {
                return new ResponseDTO<CartViewModel> { IsSuccess = true, Message = "Đã xóa sản phẩm khỏi giỏ hàng!" };
            }

            return new ResponseDTO<CartViewModel> { IsSuccess = false, Message = $"Không thể xóa: {res.StatusCode}" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cart item");
            return new ResponseDTO<CartViewModel> { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ResponseDTO<bool>> ClearCartAsync(string? token = null)
    {
        try
        {
            AttachToken(token);
            var res = await _httpClient.DeleteAsync("/api/cart/clear");
            if (!res.IsSuccessStatusCode)
            {
                res = await _httpClient.DeleteAsync("/api/cart");
            }

            if (res.IsSuccessStatusCode)
            {
                return new ResponseDTO<bool> { IsSuccess = true, Data = true, Message = "Đã xóa toàn bộ giỏ hàng!" };
            }

            return new ResponseDTO<bool> { IsSuccess = false, Message = $"Lỗi xóa giỏ hàng: {res.StatusCode}" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cart");
            return new ResponseDTO<bool> { IsSuccess = false, Message = ex.Message };
        }
    }
}
