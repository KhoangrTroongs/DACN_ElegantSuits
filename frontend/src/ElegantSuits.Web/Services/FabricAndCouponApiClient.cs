using ElegantSuits.Web.Models;

namespace ElegantSuits.Web.Services;

public interface IFabricApiClient
{
    Task<ResponseDTO<List<FabricGroupViewModel>>> GetFabricGroupsAsync();
    Task<ResponseDTO<FabricViewModel>> GetFabricByIdAsync(int id);
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
}

public interface ICouponApiClient
{
    Task<ResponseDTO<CouponViewModel>> ValidateCouponAsync(string code, decimal orderTotal);
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
}
