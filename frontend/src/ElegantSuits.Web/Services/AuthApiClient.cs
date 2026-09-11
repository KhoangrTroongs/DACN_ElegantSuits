using System.Net.Http.Json;
using System.Text.Json;
using ElegantSuits.Web.Models;

namespace ElegantSuits.Web.Services;

public interface IAuthApiClient
{
    Task<ResponseDTO<AuthResponseViewModel>> LoginAsync(LoginViewModel model);
    Task<ResponseDTO<AuthResponseViewModel>> RegisterAsync(RegisterViewModel model);
    Task<ResponseDTO<AuthResponseViewModel>> ExternalLoginAsync(ExternalLoginRequest model);
}

public class AuthApiClient : IAuthApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthApiClient> _logger;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AuthApiClient(HttpClient httpClient, ILogger<AuthApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ResponseDTO<AuthResponseViewModel>> LoginAsync(LoginViewModel model)
    {
        try
        {
            var loginDto = new
            {
                Email = model.Email?.Trim() ?? "",
                Password = model.Password ?? "",
                RememberMe = model.RememberMe
            };

            var res = await _httpClient.PostAsJsonAsync("api/Auth/login", loginDto);
            var raw = await res.Content.ReadAsStringAsync();

            if (!string.IsNullOrWhiteSpace(raw))
            {
                try
                {
                    var parsed = JsonSerializer.Deserialize<ResponseDTO<AuthResponseViewModel>>(raw, _jsonOptions);
                    if (parsed != null) return parsed;
                }
                catch { }
            }

            if (!res.IsSuccessStatusCode)
            {
                if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return new ResponseDTO<AuthResponseViewModel> { IsSuccess = false, Message = "Email hoặc mật khẩu không chính xác." };
                }
                return new ResponseDTO<AuthResponseViewModel> { IsSuccess = false, Message = $"Lỗi đăng nhập ({res.StatusCode}): {raw}" };
            }

            return new ResponseDTO<AuthResponseViewModel> { IsSuccess = false, Message = "Máy chủ trả về phản hồi không hợp lệ." };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HttpRequestException in LoginAsync");
            return new ResponseDTO<AuthResponseViewModel>
            {
                IsSuccess = false,
                Message = "Không thể kết nối đến máy chủ Backend API (http://localhost:5097). Vui lòng đảm bảo Backend đang chạy."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling login API");
            return new ResponseDTO<AuthResponseViewModel> { IsSuccess = false, Message = $"Lỗi hệ thống: {ex.Message}" };
        }
    }

    public async Task<ResponseDTO<AuthResponseViewModel>> RegisterAsync(RegisterViewModel model)
    {
        try
        {
            // Extract clean fields without IFormFile to avoid JSON serialization crash
            var registerDto = new
            {
                Email = model.Email?.Trim() ?? "",
                Password = model.Password ?? "",
                ConfirmPassword = model.ConfirmPassword ?? "",
                FullName = model.FullName?.Trim() ?? "",
                Gender = model.Gender,
                DateOfBirth = model.DateOfBirth,
                PhoneNumber = model.PhoneNumber?.Trim() ?? "",
                Address = model.Address?.Trim() ?? ""
            };

            var res = await _httpClient.PostAsJsonAsync("api/Auth/register", registerDto);
            var raw = await res.Content.ReadAsStringAsync();

            if (!string.IsNullOrWhiteSpace(raw))
            {
                try
                {
                    var parsed = JsonSerializer.Deserialize<ResponseDTO<AuthResponseViewModel>>(raw, _jsonOptions);
                    if (parsed != null) return parsed;
                }
                catch { }
            }

            if (!res.IsSuccessStatusCode)
            {
                return new ResponseDTO<AuthResponseViewModel> { IsSuccess = false, Message = $"Đăng ký không thành công ({res.StatusCode}): {raw}" };
            }

            return new ResponseDTO<AuthResponseViewModel> { IsSuccess = true, Message = "Đăng ký tài khoản thành công." };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HttpRequestException in RegisterAsync");
            return new ResponseDTO<AuthResponseViewModel>
            {
                IsSuccess = false,
                Message = "Không thể kết nối đến máy chủ Backend API (http://localhost:5097). Vui lòng đảm bảo Backend đang chạy."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling register API");
            return new ResponseDTO<AuthResponseViewModel> { IsSuccess = false, Message = $"Lỗi hệ thống: {ex.Message}" };
        }
    }

    public async Task<ResponseDTO<AuthResponseViewModel>> ExternalLoginAsync(ExternalLoginRequest model)
    {
        try
        {
            var res = await _httpClient.PostAsJsonAsync("api/Auth/external-login", model);
            var raw = await res.Content.ReadAsStringAsync();

            if (!string.IsNullOrWhiteSpace(raw))
            {
                try
                {
                    var parsed = JsonSerializer.Deserialize<ResponseDTO<AuthResponseViewModel>>(raw, _jsonOptions);
                    if (parsed != null) return parsed;
                }
                catch { }
            }

            if (!res.IsSuccessStatusCode)
            {
                return new ResponseDTO<AuthResponseViewModel> { IsSuccess = false, Message = $"Đăng nhập liên kết thất bại ({res.StatusCode}): {raw}" };
            }

            return new ResponseDTO<AuthResponseViewModel> { IsSuccess = false, Message = "Máy chủ trả về phản hồi không hợp lệ." };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HttpRequestException in ExternalLoginAsync");
            return new ResponseDTO<AuthResponseViewModel>
            {
                IsSuccess = false,
                Message = "Không thể kết nối đến máy chủ Backend API (http://localhost:5097). Vui lòng đảm bảo Backend đang chạy."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling external-login API");
            return new ResponseDTO<AuthResponseViewModel> { IsSuccess = false, Message = $"Lỗi hệ thống: {ex.Message}" };
        }
    }
}
