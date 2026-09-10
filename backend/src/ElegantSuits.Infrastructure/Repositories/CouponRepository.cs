using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Features.Coupons.Contracts;
using ElegantSuits.Domain.Entities;
using ElegantSuits.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ElegantSuits.Infrastructure.Repositories;

public class CouponRepository : ICouponRepository
{
    private readonly ApplicationDbContext _context;

    public CouponRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CouponDTO>> GetAllCouponsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Coupons
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CouponDTO
            {
                Id = c.Id,
                Code = c.Code,
                Description = c.Description,
                Quantity = c.Quantity,
                DiscountPercentage = c.DiscountPercentage,
                ExpiryDate = c.ExpiryDate,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                MinimumAmount = c.MinimumAmount
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<CouponDTO?> GetCouponByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Coupons
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CouponDTO
            {
                Id = c.Id,
                Code = c.Code,
                Description = c.Description,
                Quantity = c.Quantity,
                DiscountPercentage = c.DiscountPercentage,
                ExpiryDate = c.ExpiryDate,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                MinimumAmount = c.MinimumAmount
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<CouponDTO?> GetCouponByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Coupons
            .AsNoTracking()
            .Where(c => c.Code.ToUpper() == code.ToUpper())
            .Select(c => new CouponDTO
            {
                Id = c.Id,
                Code = c.Code,
                Description = c.Description,
                Quantity = c.Quantity,
                DiscountPercentage = c.DiscountPercentage,
                ExpiryDate = c.ExpiryDate,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                MinimumAmount = c.MinimumAmount
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ValidateCouponResult> ValidateCouponAsync(string code, decimal orderAmount, CancellationToken cancellationToken = default)
    {
        var coupon = await _context.Coupons
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == code.ToUpper(), cancellationToken);

        if (coupon == null)
        {
            return new ValidateCouponResult { IsValid = false, Message = "Mã giảm giá không tồn tại." };
        }

        if (!coupon.IsActive)
        {
            return new ValidateCouponResult { IsValid = false, Message = "Mã giảm giá đang bị khóa." };
        }

        if (DateTime.Now > coupon.ExpiryDate)
        {
            return new ValidateCouponResult { IsValid = false, Message = "Mã giảm giá đã hết hạn sử dụng." };
        }

        if (coupon.Quantity <= 0)
        {
            return new ValidateCouponResult { IsValid = false, Message = "Mã giảm giá đã hết lượt sử dụng." };
        }

        if (orderAmount < coupon.MinimumAmount)
        {
            return new ValidateCouponResult
            {
                IsValid = false,
                Message = $"Đơn hàng tối thiểu phải từ {coupon.MinimumAmount:N0} đ để áp dụng mã này."
            };
        }

        var discountAmount = (orderAmount * coupon.DiscountPercentage) / 100m;

        return new ValidateCouponResult
        {
            IsValid = true,
            Message = "Áp dụng mã giảm giá thành công.",
            DiscountPercentage = coupon.DiscountPercentage,
            DiscountAmount = discountAmount
        };
    }

    public async Task<CouponDTO> AddCouponAsync(CreateCouponDTO dto, CancellationToken cancellationToken = default)
    {
        var coupon = new Coupon
        {
            Code = dto.Code.Trim().ToUpper(),
            Description = dto.Description,
            Quantity = dto.Quantity,
            DiscountPercentage = dto.DiscountPercentage,
            ExpiryDate = dto.ExpiryDate,
            IsActive = dto.IsActive,
            MinimumAmount = dto.MinimumAmount,
            CreatedAt = DateTime.Now
        };

        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync(cancellationToken);

        return (await GetCouponByIdAsync(coupon.Id, cancellationToken))!;
    }

    public async Task<CouponDTO?> UpdateCouponAsync(int id, UpdateCouponDTO dto, CancellationToken cancellationToken = default)
    {
        var coupon = await _context.Coupons.FindAsync(new object[] { id }, cancellationToken);
        if (coupon == null) return null;

        coupon.Code = dto.Code.Trim().ToUpper();
        coupon.Description = dto.Description;
        coupon.Quantity = dto.Quantity;
        coupon.DiscountPercentage = dto.DiscountPercentage;
        coupon.ExpiryDate = dto.ExpiryDate;
        coupon.IsActive = dto.IsActive;
        coupon.MinimumAmount = dto.MinimumAmount;
        coupon.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync(cancellationToken);
        return await GetCouponByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteCouponAsync(int id, CancellationToken cancellationToken = default)
    {
        var coupon = await _context.Coupons.FindAsync(new object[] { id }, cancellationToken);
        if (coupon == null) return false;

        _context.Coupons.Remove(coupon);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
