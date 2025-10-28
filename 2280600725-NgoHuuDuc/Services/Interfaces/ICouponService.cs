using NgoHuuDuc_2280600725.DTOs;

namespace NgoHuuDuc_2280600725.Services.Interfaces
{
    public interface ICouponService
    {
        Task<IEnumerable<CouponDTO>> GetAllCouponsAsync();
        Task<CouponDTO?> GetCouponByIdAsync(int id);
        Task<CouponDTO?> GetCouponByCodeAsync(string code);
        Task<CouponDTO> AddCouponAsync(CreateCouponDTO couponDto);
        Task<CouponDTO?> UpdateCouponAsync(int id, UpdateCouponDTO couponDto);
        Task<bool> DeleteCouponAsync(int id);
        Task<bool> CouponExistsAsync(int id);
        Task<bool> CouponCodeExistsAsync(string code, int excludeId = 0);
        Task<CouponValidationResult> ValidateCouponAsync(string code, decimal cartTotal = 0);
        Task<bool> DecrementCouponQuantityAsync(string code);
        Task<string> GenerateUniqueCouponCodeAsync();
        Task<List<CouponDTO>> GetAvailableCouponsAsync(decimal cartTotal, bool hasFreeship, List<int> productIds);
    }

    public class CouponValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public CouponDTO? Coupon { get; set; }
    }
}

