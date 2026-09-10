using ElegantSuits.Web.Models;

namespace ElegantSuits.Web.Services;

public interface IAuthApiClient
{
    Task<ResponseDTO<AuthResponseViewModel>> LoginAsync(LoginViewModel model);
    Task<ResponseDTO<AuthResponseViewModel>> RegisterAsync(RegisterViewModel model);
}

public class AuthApiClient : IAuthApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthApiClient> _logger;

    public AuthApiClient(HttpClient httpClient, ILogger<AuthApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ResponseDTO<AuthResponseViewModel>> LoginAsync(LoginViewModel model)
    {
        try
        {
            var res = await _httpClient.PostAsJsonAsync("/api/auth/login", model);
            return await res.Content.ReadFromJsonAsync<ResponseDTO<AuthResponseViewModel>>()
                ?? new ResponseDTO<AuthResponseViewModel> { IsSuccess = false, Message = "No response" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling login API");
            return new ResponseDTO<AuthResponseViewModel> { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ResponseDTO<AuthResponseViewModel>> RegisterAsync(RegisterViewModel model)
    {
        try
        {
            var res = await _httpClient.PostAsJsonAsync("/api/auth/register", model);
            return await res.Content.ReadFromJsonAsync<ResponseDTO<AuthResponseViewModel>>()
                ?? new ResponseDTO<AuthResponseViewModel> { IsSuccess = false, Message = "No response" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling register API");
            return new ResponseDTO<AuthResponseViewModel> { IsSuccess = false, Message = ex.Message };
        }
    }
}
