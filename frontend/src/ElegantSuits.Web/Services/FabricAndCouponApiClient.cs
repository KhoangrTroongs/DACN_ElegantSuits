using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ElegantSuits.Web.Models;

namespace ElegantSuits.Web.Services;

public interface IFabricApiClient
{
    Task<ResponseDTO<List<FabricGroupViewModel>>> GetFabricGroupsAsync();
    Task<ResponseDTO<FabricViewModel>> GetFabricByIdAsync(int id);
    Task<ResponseDTO<FabricDTO>> CreateFabricAsync(CreateFabricDTO dto, string? token = null);
    Task<ResponseDTO<FabricDTO>> UpdateFabricAsync(int id, UpdateFabricDTO dto, string? token = null);
    Task<ResponseDTO<bool>> DeleteFabricAsync(int id, string? token = null);
    Task<ResponseDTO<FabricGroupDTO>> CreateFabricGroupAsync(CreateFabricGroupDTO dto, string? token = null);
    Task<ResponseDTO<FabricGroupDTO>> UpdateFabricGroupAsync(int id, UpdateFabricGroupDTO dto, string? token = null);
    Task<ResponseDTO<bool>> DeleteFabricGroupAsync(int id, string? token = null);
}

public class FabricApiClient : IFabricApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FabricApiClient> _logger;

    public FabricApiClient(HttpClient httpClient, ILogger<FabricApiClient> logger)
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

    public async Task<ResponseDTO<List<FabricGroupViewModel>>> GetFabricGroupsAsync()
    {
        try
        {
            var res = await _httpClient.GetFromJsonAsync<ResponseDTO<List<FabricGroupViewModel>>>("/api/fabrics/groups");
            return res ?? new ResponseDTO<List<FabricGroupViewModel>> { IsSuccess = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting fabric groups");
            return new ResponseDTO<List<FabricGroupViewModel>> { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ResponseDTO<FabricViewModel>> GetFabricByIdAsync(int id)
    {
        try
        {
            var groupsRes = await GetFabricGroupsAsync();
            if (groupsRes.IsSuccess && groupsRes.Data != null)
            {
                var fabric = groupsRes.Data
                    .SelectMany(g => g.Fabrics)
                    .FirstOrDefault(f => f.Id == id);
                if (fabric != null)
                    return ResponseDTO<FabricViewModel>.Success(fabric);
            }
            return new ResponseDTO<FabricViewModel> { IsSuccess = false, Message = "Fabric not found" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting fabric {Id}", id);
            return new ResponseDTO<FabricViewModel> { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ResponseDTO<FabricDTO>> CreateFabricAsync(CreateFabricDTO dto, string? token = null)
    {
        try
        {
            AttachToken(token);
            // Send as multipart if image is provided, otherwise JSON
            HttpResponseMessage res;
            if (dto.Image != null)
            {
                using var form = new MultipartFormDataContent();
                form.Add(new StringContent(dto.Name), "Name");
                form.Add(new StringContent(dto.Description ?? ""), "Description");
                form.Add(new StringContent(dto.Composition ?? ""), "Composition");
                form.Add(new StringContent(dto.ImageUrl ?? ""), "ImageUrl");
                form.Add(new StringContent(dto.Price.ToString()), "Price");
                form.Add(new StringContent(dto.FabricGroupId.ToString()), "FabricGroupId");
                var stream = dto.Image.OpenReadStream();
                form.Add(new StreamContent(stream), "Image", dto.Image.FileName);
                res = await _httpClient.PostAsync("/api/fabrics", form);
            }
            else
            {
                res = await _httpClient.PostAsJsonAsync("/api/fabrics", new
                {
                    dto.Name, dto.Description, dto.Composition, dto.ImageUrl, dto.Price, dto.FabricGroupId
                });
            }

            if (res.IsSuccessStatusCode)
            {
                var result = await res.Content.ReadFromJsonAsync<ResponseDTO<FabricDTO>>();
                return result ?? ResponseDTO<FabricDTO>.Fail("Lỗi phân tích phản hồi");
            }
            var err = await res.Content.ReadFromJsonAsync<ResponseDTO<FabricDTO>>();
            return err ?? ResponseDTO<FabricDTO>.Fail($"Lỗi: {res.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating fabric");
            return ResponseDTO<FabricDTO>.Fail(ex.Message);
        }
    }

    public async Task<ResponseDTO<FabricDTO>> UpdateFabricAsync(int id, UpdateFabricDTO dto, string? token = null)
    {
        try
        {
            AttachToken(token);
            var res = await _httpClient.PutAsJsonAsync($"/api/fabrics/{id}", new
            {
                dto.Name, dto.Description, dto.Composition, dto.ImageUrl, dto.Price, dto.FabricGroupId, dto.IsAvailable
            });

            if (res.IsSuccessStatusCode)
            {
                var result = await res.Content.ReadFromJsonAsync<ResponseDTO<FabricDTO>>();
                return result ?? ResponseDTO<FabricDTO>.Fail("Lỗi phân tích phản hồi");
            }
            var err = await res.Content.ReadFromJsonAsync<ResponseDTO<FabricDTO>>();
            return err ?? ResponseDTO<FabricDTO>.Fail($"Lỗi: {res.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating fabric {Id}", id);
            return ResponseDTO<FabricDTO>.Fail(ex.Message);
        }
    }

    public async Task<ResponseDTO<bool>> DeleteFabricAsync(int id, string? token = null)
    {
        try
        {
            AttachToken(token);
            var res = await _httpClient.DeleteAsync($"/api/fabrics/{id}");
            if (res.IsSuccessStatusCode)
                return ResponseDTO<bool>.Success(true);
            var err = await res.Content.ReadFromJsonAsync<ResponseDTO<bool>>();
            return err ?? ResponseDTO<bool>.Fail($"Lỗi: {res.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting fabric {Id}", id);
            return ResponseDTO<bool>.Fail(ex.Message);
        }
    }

    public async Task<ResponseDTO<FabricGroupDTO>> CreateFabricGroupAsync(CreateFabricGroupDTO dto, string? token = null)
    {
        try
        {
            AttachToken(token);
            var res = await _httpClient.PostAsJsonAsync("/api/fabrics/groups", dto);
            if (res.IsSuccessStatusCode)
            {
                var result = await res.Content.ReadFromJsonAsync<ResponseDTO<FabricGroupDTO>>();
                return result ?? ResponseDTO<FabricGroupDTO>.Fail("Lỗi phân tích phản hồi");
            }
            var err = await res.Content.ReadFromJsonAsync<ResponseDTO<FabricGroupDTO>>();
            return err ?? ResponseDTO<FabricGroupDTO>.Fail($"Lỗi: {res.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating fabric group");
            return ResponseDTO<FabricGroupDTO>.Fail(ex.Message);
        }
    }

    public async Task<ResponseDTO<FabricGroupDTO>> UpdateFabricGroupAsync(int id, UpdateFabricGroupDTO dto, string? token = null)
    {
        try
        {
            AttachToken(token);
            var res = await _httpClient.PutAsJsonAsync($"/api/fabrics/groups/{id}", dto);
            if (res.IsSuccessStatusCode)
            {
                var result = await res.Content.ReadFromJsonAsync<ResponseDTO<FabricGroupDTO>>();
                return result ?? ResponseDTO<FabricGroupDTO>.Fail("Lỗi phân tích phản hồi");
            }
            var err = await res.Content.ReadFromJsonAsync<ResponseDTO<FabricGroupDTO>>();
            return err ?? ResponseDTO<FabricGroupDTO>.Fail($"Lỗi: {res.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating fabric group {Id}", id);
            return ResponseDTO<FabricGroupDTO>.Fail(ex.Message);
        }
    }

    public async Task<ResponseDTO<bool>> DeleteFabricGroupAsync(int id, string? token = null)
    {
        try
        {
            AttachToken(token);
            var res = await _httpClient.DeleteAsync($"/api/fabrics/groups/{id}");
            if (res.IsSuccessStatusCode)
                return ResponseDTO<bool>.Success(true);
            var err = await res.Content.ReadFromJsonAsync<ResponseDTO<bool>>();
            return err ?? ResponseDTO<bool>.Fail($"Lỗi: {res.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting fabric group {Id}", id);
            return ResponseDTO<bool>.Fail(ex.Message);
        }
    }
}

public interface ICouponApiClient
{
    Task<ResponseDTO<CouponViewModel>> ValidateCouponAsync(string code, decimal orderTotal);
    Task<ResponseDTO<List<CouponDTO>>> GetAllCouponsAsync(string? token = null);
    Task<ResponseDTO<CouponDTO>> GetCouponByIdAsync(int id, string? token = null);
    Task<ResponseDTO<CouponDTO>> CreateCouponAsync(CreateCouponDTO dto, string? token = null);
    Task<ResponseDTO<CouponDTO>> UpdateCouponAsync(int id, UpdateCouponDTO dto, string? token = null);
    Task<ResponseDTO<bool>> DeleteCouponAsync(int id, string? token = null);
}

public class CouponApiClient : ICouponApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CouponApiClient> _logger;

    public CouponApiClient(HttpClient httpClient, ILogger<CouponApiClient> logger)
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

    public async Task<ResponseDTO<CouponViewModel>> ValidateCouponAsync(string code, decimal orderTotal)
    {
        try
        {
            var res = await _httpClient.PostAsJsonAsync("/api/CouponApi/validate",
                new { Code = code, OrderAmount = orderTotal });

            if (!res.IsSuccessStatusCode)
            {
                var errBody = await res.Content.ReadFromJsonAsync<ResponseDTO<CouponViewModel>>();
                return errBody ?? new ResponseDTO<CouponViewModel> { IsSuccess = false, Message = "Coupon khong hop le" };
            }

            var result = await res.Content.ReadFromJsonAsync<ResponseDTO<CouponViewModel>>();
            return result ?? new ResponseDTO<CouponViewModel> { IsSuccess = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating coupon {Code}", code);
            return new ResponseDTO<CouponViewModel> { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ResponseDTO<List<CouponDTO>>> GetAllCouponsAsync(string? token = null)
    {
        try
        {
            AttachToken(token);
            var res = await _httpClient.GetAsync("/api/CouponApi");
            if (res.IsSuccessStatusCode)
            {
                var content = await res.Content.ReadAsStringAsync();
                try
                {
                    var responseDto = JsonSerializer.Deserialize<ResponseDTO<List<CouponDTO>>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (responseDto != null && responseDto.Data != null)
                    {
                        return responseDto;
                    }
                }
                catch { }

                try
                {
                    var rawList = JsonSerializer.Deserialize<List<CouponDTO>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (rawList != null)
                    {
                        return ResponseDTO<List<CouponDTO>>.Success(rawList);
                    }
                }
                catch { }
            }
            return new ResponseDTO<List<CouponDTO>> { IsSuccess = false, Message = "Không thể tải danh sách mã giảm giá" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all coupons");
            return new ResponseDTO<List<CouponDTO>> { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ResponseDTO<CouponDTO>> GetCouponByIdAsync(int id, string? token = null)
    {
        try
        {
            AttachToken(token);
            var res = await _httpClient.GetAsync($"/api/CouponApi/{id}");
            if (res.IsSuccessStatusCode)
            {
                var result = await res.Content.ReadFromJsonAsync<ResponseDTO<CouponDTO>>();
                return result ?? new ResponseDTO<CouponDTO> { IsSuccess = false, Message = "Không tìm thấy mã giảm giá" };
            }
            return new ResponseDTO<CouponDTO> { IsSuccess = false, Message = "Không tìm thấy mã giảm giá" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting coupon {Id}", id);
            return new ResponseDTO<CouponDTO> { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ResponseDTO<CouponDTO>> CreateCouponAsync(CreateCouponDTO dto, string? token = null)
    {
        try
        {
            AttachToken(token);
            var res = await _httpClient.PostAsJsonAsync("/api/CouponApi", dto);
            if (!res.IsSuccessStatusCode)
            {
                var errBody = await res.Content.ReadFromJsonAsync<ResponseDTO<CouponDTO>>();
                return errBody ?? new ResponseDTO<CouponDTO> { IsSuccess = false, Message = $"Lỗi: {res.StatusCode}" };
            }

            var result = await res.Content.ReadFromJsonAsync<ResponseDTO<CouponDTO>>();
            return result ?? new ResponseDTO<CouponDTO> { IsSuccess = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating coupon");
            return new ResponseDTO<CouponDTO> { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ResponseDTO<CouponDTO>> UpdateCouponAsync(int id, UpdateCouponDTO dto, string? token = null)
    {
        try
        {
            AttachToken(token);
            var res = await _httpClient.PutAsJsonAsync($"/api/CouponApi/{id}", dto);
            if (!res.IsSuccessStatusCode)
            {
                var errBody = await res.Content.ReadFromJsonAsync<ResponseDTO<CouponDTO>>();
                return errBody ?? new ResponseDTO<CouponDTO> { IsSuccess = false, Message = $"Lỗi: {res.StatusCode}" };
            }

            var result = await res.Content.ReadFromJsonAsync<ResponseDTO<CouponDTO>>();
            return result ?? new ResponseDTO<CouponDTO> { IsSuccess = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating coupon {Id}", id);
            return new ResponseDTO<CouponDTO> { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ResponseDTO<bool>> DeleteCouponAsync(int id, string? token = null)
    {
        try
        {
            AttachToken(token);
            var res = await _httpClient.DeleteAsync($"/api/CouponApi/{id}");
            if (res.IsSuccessStatusCode)
            {
                var result = await res.Content.ReadFromJsonAsync<ResponseDTO<bool>>();
                return result ?? ResponseDTO<bool>.Success(true);
            }

            var err = await res.Content.ReadFromJsonAsync<ResponseDTO<bool>>();
            return err ?? new ResponseDTO<bool> { IsSuccess = false, Message = "Không thể xóa mã giảm giá" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting coupon {Id}", id);
            return new ResponseDTO<bool> { IsSuccess = false, Message = ex.Message };
        }
    }
}
