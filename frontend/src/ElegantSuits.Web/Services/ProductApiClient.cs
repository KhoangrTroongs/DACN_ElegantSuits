using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ElegantSuits.Web.Models;

namespace ElegantSuits.Web.Services;

public class ProductApiClient : IProductApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ProductApiClient> _logger;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ProductApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<ProductApiClient> logger)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    private void AttachAuthToken()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var token = httpContext?.Session.GetString("JwtToken")
                 ?? httpContext?.User.FindFirst("JwtToken")?.Value;

        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            if (httpContext?.Session.GetString("JwtToken") == null)
            {
                httpContext?.Session.SetString("JwtToken", token);
            }
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    public async Task<IReadOnlyList<ProductViewModel>> GetProductsAsync(int? categoryId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            AttachAuthToken();
            var url = categoryId.HasValue ? $"api/Products?categoryId={categoryId.Value}" : "api/Products";
            var res = await _httpClient.GetFromJsonAsync<ResponseDTO<List<ProductViewModel>>>(url, _jsonOptions, cancellationToken);
            return res?.Data ?? new List<ProductViewModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling GetProductsAsync");
            return new List<ProductViewModel>();
        }
    }

    public async Task<PaginatedList<ProductViewModel>?> GetPagedProductsAsync(int? categoryId = null, int pageIndex = 1, int pageSize = 10, string? keyword = null, CancellationToken cancellationToken = default)
    {
        try
        {
            AttachAuthToken();
            var url = $"api/Products/paged?pageIndex={pageIndex}&pageSize={pageSize}";
            if (categoryId.HasValue) url += $"&categoryId={categoryId.Value}";
            if (!string.IsNullOrWhiteSpace(keyword)) url += $"&keyword={Uri.EscapeDataString(keyword)}";

            var res = await _httpClient.GetFromJsonAsync<ResponseDTO<PaginatedList<ProductViewModel>>>(url, _jsonOptions, cancellationToken);
            return res?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling GetPagedProductsAsync");
            return null;
        }
    }

    public async Task<ProductViewModel?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            AttachAuthToken();
            var res = await _httpClient.GetFromJsonAsync<ResponseDTO<ProductViewModel>>($"api/Products/{id}", _jsonOptions, cancellationToken);
            return res?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling GetProductByIdAsync for id {Id}", id);
            return null;
        }
    }

    public async Task<PaginatedList<ProductViewModel>?> SearchProductsAsync(string keyword, int pageIndex = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            AttachAuthToken();
            var url = $"api/Products/search?keyword={Uri.EscapeDataString(keyword)}&pageIndex={pageIndex}&pageSize={pageSize}";
            var res = await _httpClient.GetFromJsonAsync<ResponseDTO<PaginatedList<ProductViewModel>>>(url, _jsonOptions, cancellationToken);
            return res?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling SearchProductsAsync for keyword {Keyword}", keyword);
            return null;
        }
    }

    public async Task<ResponseDTO<ProductViewModel>> CreateProductAsync(CreateProductViewModel model, CancellationToken cancellationToken = default)
    {
        try
        {
            AttachAuthToken();
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(model.Name ?? ""), "Name");
            content.Add(new StringContent(model.Description ?? ""), "Description");
            content.Add(new StringContent(model.Price.ToString(CultureInfo.InvariantCulture)), "Price");
            content.Add(new StringContent(model.Quantity.ToString()), "Quantity");
            content.Add(new StringContent(model.CategoryId.ToString()), "CategoryId");
            content.Add(new StringContent(model.IsHidden.ToString()), "IsHidden");
            content.Add(new StringContent(model.ProfitMargin.ToString(CultureInfo.InvariantCulture)), "ProfitMargin");
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

            if (model.Model3D != null)
            {
                var modelStreamContent = new StreamContent(model.Model3D.OpenReadStream());
                modelStreamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                content.Add(modelStreamContent, "model3D", model.Model3D.FileName);
            }

            var response = await _httpClient.PostAsync("api/Products", content, cancellationToken);
            var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("CreateProduct returned status {Status}: {Content}", response.StatusCode, rawContent);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return ResponseDTO<ProductViewModel>.Fail("Phiên đăng nhập đã hết hạn hoặc chưa đăng nhập. Vui lòng đăng nhập lại tài khoản Quản trị viên.");
                }
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return ResponseDTO<ProductViewModel>.Fail("Tài khoản của bạn không có quyền Administrator để tạo sản phẩm.");
                }

                if (!string.IsNullOrWhiteSpace(rawContent))
                {
                    try
                    {
                        var errorDto = JsonSerializer.Deserialize<ResponseDTO<ProductViewModel>>(rawContent, _jsonOptions);
                        if (errorDto != null) return errorDto;
                    }
                    catch { }
                    return ResponseDTO<ProductViewModel>.Fail($"Lỗi từ máy chủ ({response.StatusCode}): {rawContent}");
                }
                return ResponseDTO<ProductViewModel>.Fail($"Lỗi máy chủ ({response.StatusCode})");
            }

            if (string.IsNullOrWhiteSpace(rawContent))
            {
                return ResponseDTO<ProductViewModel>.Fail("Máy chủ trả về dữ liệu rỗng.");
            }

            var result = JsonSerializer.Deserialize<ResponseDTO<ProductViewModel>>(rawContent, _jsonOptions);
            return result ?? ResponseDTO<ProductViewModel>.Fail("Không nhận được dữ liệu từ máy chủ.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception calling CreateProductAsync");
            return ResponseDTO<ProductViewModel>.Fail($"Lỗi kết nối máy chủ: {ex.Message}");
        }
    }

    public async Task<ResponseDTO<ProductViewModel>> UpdateProductAsync(int id, UpdateProductViewModel model, CancellationToken cancellationToken = default)
    {
        try
        {
            AttachAuthToken();
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(model.Name ?? ""), "Name");
            content.Add(new StringContent(model.Description ?? ""), "Description");
            content.Add(new StringContent(model.Price.ToString(CultureInfo.InvariantCulture)), "Price");
            content.Add(new StringContent(model.Quantity.ToString()), "Quantity");
            content.Add(new StringContent(model.CategoryId.ToString()), "CategoryId");
            content.Add(new StringContent(model.IsHidden.ToString()), "IsHidden");
            content.Add(new StringContent(model.ProfitMargin.ToString(CultureInfo.InvariantCulture)), "ProfitMargin");
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

            if (model.Model3D != null)
            {
                var modelStreamContent = new StreamContent(model.Model3D.OpenReadStream());
                modelStreamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                content.Add(modelStreamContent, "model3D", model.Model3D.FileName);
            }

            var response = await _httpClient.PutAsync($"api/Products/{id}", content, cancellationToken);
            var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("UpdateProduct returned status {Status}: {Content}", response.StatusCode, rawContent);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return ResponseDTO<ProductViewModel>.Fail("Phiên đăng nhập đã hết hạn hoặc chưa đăng nhập. Vui lòng đăng nhập lại tài khoản Quản trị viên.");
                }
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return ResponseDTO<ProductViewModel>.Fail("Tài khoản của bạn không có quyền Administrator để sửa sản phẩm.");
                }

                if (!string.IsNullOrWhiteSpace(rawContent))
                {
                    try
                    {
                        var errorDto = JsonSerializer.Deserialize<ResponseDTO<ProductViewModel>>(rawContent, _jsonOptions);
                        if (errorDto != null) return errorDto;
                    }
                    catch { }
                    return ResponseDTO<ProductViewModel>.Fail($"Lỗi từ máy chủ ({response.StatusCode}): {rawContent}");
                }
                return ResponseDTO<ProductViewModel>.Fail($"Lỗi máy chủ ({response.StatusCode})");
            }

            if (string.IsNullOrWhiteSpace(rawContent))
            {
                return ResponseDTO<ProductViewModel>.Fail("Máy chủ trả về dữ liệu rỗng.");
            }

            var result = JsonSerializer.Deserialize<ResponseDTO<ProductViewModel>>(rawContent, _jsonOptions);
            return result ?? ResponseDTO<ProductViewModel>.Fail("Không nhận được dữ liệu từ máy chủ.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception calling UpdateProductAsync for id {Id}", id);
            return ResponseDTO<ProductViewModel>.Fail($"Lỗi kết nối máy chủ: {ex.Message}");
        }
    }

    public async Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            AttachAuthToken();
            var response = await _httpClient.DeleteAsync($"api/Products/{id}", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception calling DeleteProductAsync for id {Id}", id);
            return false;
        }
    }

    public async Task<ResponseDTO<ProductReviewResponse>> AddReviewAsync(int productId, int rating, string? comment, string? token = null)
    {
        try
        {
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                AttachAuthToken();
            }

            var response = await _httpClient.PostAsJsonAsync($"api/Products/{productId}/reviews", new { rating, comment });
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ResponseDTO<ProductReviewResponse>>(_jsonOptions);
                return result ?? ResponseDTO<ProductReviewResponse>.Success(new ProductReviewResponse(), "Cảm ơn bạn đã đánh giá!");
            }

            var err = await response.Content.ReadFromJsonAsync<ResponseDTO<ProductReviewResponse>>(_jsonOptions);
            return err ?? ResponseDTO<ProductReviewResponse>.Fail($"Lỗi: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception calling AddReviewAsync for productId {ProductId}", productId);
            return ResponseDTO<ProductReviewResponse>.Fail($"Lỗi kết nối máy chủ: {ex.Message}");
        }
    }

    public async Task<ResponseDTO<bool>> DeleteReviewAsync(int productId, int reviewId, string? token = null)
    {
        try
        {
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                AttachAuthToken();
            }

            var response = await _httpClient.DeleteAsync($"api/Products/{productId}/reviews/{reviewId}");
            if (response.IsSuccessStatusCode)
            {
                return ResponseDTO<bool>.Success(true, "Đã xóa đánh giá thành công.");
            }

            return ResponseDTO<bool>.Fail($"Lỗi: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception calling DeleteReviewAsync for productId {ProductId}, reviewId {ReviewId}", productId, reviewId);
            return ResponseDTO<bool>.Fail($"Lỗi kết nối máy chủ: {ex.Message}");
        }
    }
}
