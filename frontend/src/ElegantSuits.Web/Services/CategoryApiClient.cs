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
            return await res.Content.ReadFromJsonAsync<ResponseDTO<int>>()
                ?? new ResponseDTO<int> { IsSuccess = false };
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
