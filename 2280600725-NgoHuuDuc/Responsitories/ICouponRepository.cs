using NgoHuuDuc_2280600725.Models;

namespace NgoHuuDuc_2280600725.Responsitories
{
    public interface ICouponRepository
    {
        Task<IEnumerable<Coupon>> GetAllCouponsAsync();
        Task<Coupon?> GetCouponByIdAsync(int id);
        Task<Coupon?> GetCouponByCodeAsync(string code);
        Task<Coupon> AddCouponAsync(Coupon coupon);
        Task<Coupon> UpdateCouponAsync(Coupon coupon);
        Task DeleteCouponAsync(int id);
        Task<bool> CouponExistsAsync(int id);
        Task<bool> CouponCodeExistsAsync(string code, int excludeId = 0);
    }
}

