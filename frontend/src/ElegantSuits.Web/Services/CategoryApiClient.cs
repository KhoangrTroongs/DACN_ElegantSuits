using System.Text.Json;
using ElegantSuits.Web.Models;

namespace ElegantSuits.Web.Services;

public interface ICategoryApiClient
{
    Task<ResponseDTO<List<CategoryViewModel>>> GetAllCategoriesAsync();
    Task<ResponseDTO<CategoryViewModel>> GetCategoryByIdAsync(int id);
    Task<ResponseDTO<int>> CreateCategoryAsync(CategoryViewModel model);
    Task<ResponseDTO<bool>> UpdateCategoryAsync(int id, CategoryViewModel model);
    Task<ResponseDTO<bool>> DeleteCategoryAsync(int id);
}

public class CategoryApiClient : ICategoryApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CategoryApiClient> _logger;

    public CategoryApiClient(HttpClient httpClient, ILogger<CategoryApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ResponseDTO<List<CategoryViewModel>>> GetAllCategoriesAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ResponseDTO<List<CategoryViewModel>>>("/api/categories");
            return response ?? new ResponseDTO<List<CategoryViewModel>> { IsSuccess = false, Message = "Empty response" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling GET /api/categories");
            return new ResponseDTO<List<CategoryViewModel>> { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ResponseDTO<CategoryViewModel>> GetCategoryByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ResponseDTO<CategoryViewModel>>($"/api/categories/{id}");
            return response ?? new ResponseDTO<CategoryViewModel> { IsSuccess = false, Message = "Not found" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling GET /api/categories/{id}", id);
            return new ResponseDTO<CategoryViewModel> { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ResponseDTO<int>> CreateCategoryAsync(CategoryViewModel model)
    {
        try
        {
            var res = await _httpClient.PostAsJsonAsync("/api/categories", model);
            var json = await res.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json))
            {
                return new ResponseDTO<int> { IsSuccess = res.IsSuccessStatusCode };
            }

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            bool isSuccess = (root.TryGetProperty("isSuccess", out var s1) || root.TryGetProperty("IsSuccess", out s1)) ? s1.GetBoolean() : res.IsSuccessStatusCode;
            string? message = (root.TryGetProperty("message", out var m1) || root.TryGetProperty("Message", out m1)) && m1.ValueKind == JsonValueKind.String ? m1.GetString() : null;

            int catId = 0;
            if (root.TryGetProperty("data", out var d1) || root.TryGetProperty("Data", out d1))
            {
                if (d1.ValueKind == JsonValueKind.Number) catId = d1.GetInt32();
                else if (d1.ValueKind == JsonValueKind.Object && (d1.TryGetProperty("id", out var idProp) || d1.TryGetProperty("Id", out idProp)) && idProp.ValueKind == JsonValueKind.Number)
                {
                    catId = idProp.GetInt32();
                }
            }

            return new ResponseDTO<int> { IsSuccess = isSuccess, Message = message, Data = catId };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling POST /api/categories");
            return new ResponseDTO<int> { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ResponseDTO<bool>> UpdateCategoryAsync(int id, CategoryViewModel model)
    {
        try
        {
            var res = await _httpClient.PutAsJsonAsync($"/api/categories/{id}", model);
            return await res.Content.ReadFromJsonAsync<ResponseDTO<bool>>()
                ?? new ResponseDTO<bool> { IsSuccess = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling PUT /api/categories/{id}", id);
            return new ResponseDTO<bool> { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ResponseDTO<bool>> DeleteCategoryAsync(int id)
    {
        try
        {
            var res = await _httpClient.DeleteAsync($"/api/categories/{id}");
            return await res.Content.ReadFromJsonAsync<ResponseDTO<bool>>()
                ?? new ResponseDTO<bool> { IsSuccess = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling DELETE /api/categories/{id}", id);
            return new ResponseDTO<bool> { IsSuccess = false, Message = ex.Message };
        }
    }
}
