using ElegantSuits.Application.Features.Coupons.Contracts;
using ElegantSuits.Domain.Entities;

namespace ElegantSuits.Application.Common.Interfaces;

public interface ICouponRepository
{
    Task<IEnumerable<CouponDTO>> GetAllCouponsAsync(CancellationToken cancellationToken = default);
    Task<CouponDTO?> GetCouponByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CouponDTO?> GetCouponByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<ValidateCouponResult> ValidateCouponAsync(string code, decimal orderAmount, CancellationToken cancellationToken = default);
    Task<CouponDTO> AddCouponAsync(CreateCouponDTO dto, CancellationToken cancellationToken = default);
    Task<CouponDTO?> UpdateCouponAsync(int id, UpdateCouponDTO dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteCouponAsync(int id, CancellationToken cancellationToken = default);
}
