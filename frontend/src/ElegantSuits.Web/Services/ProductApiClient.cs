using System.Net.Http.Headers;
using System.Net.Http.Json;
using ElegantSuits.Web.Models;

namespace ElegantSuits.Web.Services;

public class ProductApiClient : IProductApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProductApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    private void AttachAuthToken()
    {
        var token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<IReadOnlyList<ProductViewModel>> GetProductsAsync(int? categoryId = null, CancellationToken cancellationToken = default)
    {
        AttachAuthToken();
        var url = categoryId.HasValue ? $"api/Products?categoryId={categoryId.Value}" : "api/Products";
        var res = await _httpClient.GetFromJsonAsync<ResponseDTO<List<ProductViewModel>>>(url, cancellationToken);
        return res?.Data ?? new List<ProductViewModel>();
    }

    public async Task<PaginatedList<ProductViewModel>?> GetPagedProductsAsync(int? categoryId = null, int pageIndex = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        AttachAuthToken();
        var url = $"api/Products/paged?pageIndex={pageIndex}&pageSize={pageSize}";
        if (categoryId.HasValue) url += $"&categoryId={categoryId.Value}";

        var res = await _httpClient.GetFromJsonAsync<ResponseDTO<PaginatedList<ProductViewModel>>>(url, cancellationToken);
        return res?.Data;
    }

    public async Task<ProductViewModel?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        AttachAuthToken();
        var res = await _httpClient.GetFromJsonAsync<ResponseDTO<ProductViewModel>>($"api/Products/{id}", cancellationToken);
        return res?.Data;
    }

    public async Task<PaginatedList<ProductViewModel>?> SearchProductsAsync(string keyword, int pageIndex = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        AttachAuthToken();
        var url = $"api/Products/search?keyword={Uri.EscapeDataString(keyword)}&pageIndex={pageIndex}&pageSize={pageSize}";
        var res = await _httpClient.GetFromJsonAsync<ResponseDTO<PaginatedList<ProductViewModel>>>(url, cancellationToken);
        return res?.Data;
    }

    public async Task<ResponseDTO<ProductViewModel>> CreateProductAsync(CreateProductViewModel model, CancellationToken cancellationToken = default)
    {
        AttachAuthToken();
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(model.Name), "Name");
        content.Add(new StringContent(model.Description ?? ""), "Description");
        content.Add(new StringContent(model.Price.ToString()), "Price");
        content.Add(new StringContent(model.Quantity.ToString()), "Quantity");
        content.Add(new StringContent(model.CategoryId.ToString()), "CategoryId");
        content.Add(new StringContent(model.IsHidden.ToString()), "IsHidden");
        content.Add(new StringContent(model.ProfitMargin.ToString()), "ProfitMargin");
        if (!string.IsNullOrEmpty(model.LinearCode))
            content.Add(new StringContent(model.LinearCode), "LinearCode");
        if (!string.IsNullOrEmpty(model.Model3DUrl))
            content.Add(new StringContent(model.Model3DUrl), "Model3DUrl");

        if (model.Image != null)
        {
            var streamContent = new StreamContent(model.Image.OpenReadStream());
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(model.Image.ContentType);
            content.Add(streamContent, "image", model.Image.FileName);
        }

        var response = await _httpClient.PostAsync("api/Products", content, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ResponseDTO<ProductViewModel>>(cancellationToken: cancellationToken);
        return result ?? ResponseDTO<ProductViewModel>.Fail("No response from server.");
    }

    public async Task<ResponseDTO<ProductViewModel>> UpdateProductAsync(int id, UpdateProductViewModel model, CancellationToken cancellationToken = default)
    {
        AttachAuthToken();
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(model.Name), "Name");
        content.Add(new StringContent(model.Description ?? ""), "Description");
        content.Add(new StringContent(model.Price.ToString()), "Price");
        content.Add(new StringContent(model.Quantity.ToString()), "Quantity");
        content.Add(new StringContent(model.CategoryId.ToString()), "CategoryId");
        content.Add(new StringContent(model.IsHidden.ToString()), "IsHidden");
        content.Add(new StringContent(model.ProfitMargin.ToString()), "ProfitMargin");
        if (!string.IsNullOrEmpty(model.ImageUrl))
            content.Add(new StringContent(model.ImageUrl), "ImageUrl");
        if (!string.IsNullOrEmpty(model.LinearCode))
            content.Add(new StringContent(model.LinearCode), "LinearCode");
        if (!string.IsNullOrEmpty(model.Model3DUrl))
            content.Add(new StringContent(model.Model3DUrl), "Model3DUrl");

        if (model.Image != null)
        {
            var streamContent = new StreamContent(model.Image.OpenReadStream());
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(model.Image.ContentType);
            content.Add(streamContent, "image", model.Image.FileName);
        }

        var response = await _httpClient.PutAsync($"api/Products/{id}", content, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ResponseDTO<ProductViewModel>>(cancellationToken: cancellationToken);
        return result ?? ResponseDTO<ProductViewModel>.Fail("No response from server.");
    }

    public async Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default)
    {
        AttachAuthToken();
        var response = await _httpClient.DeleteAsync($"api/Products/{id}", cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
