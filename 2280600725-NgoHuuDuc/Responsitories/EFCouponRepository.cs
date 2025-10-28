using Microsoft.EntityFrameworkCore;
using NgoHuuDuc_2280600725.Data;
using NgoHuuDuc_2280600725.Models;

namespace NgoHuuDuc_2280600725.Responsitories
{
    public class EFCouponRepository : ICouponRepository
    {
        private readonly ApplicationDbContext _context;

        public EFCouponRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Coupon>> GetAllCouponsAsync()
        {
            return await _context.Coupons
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Coupon?> GetCouponByIdAsync(int id)
        {
            return await _context.Coupons.FindAsync(id);
        }

        public async Task<Coupon?> GetCouponByCodeAsync(string code)
        {
            // Case-insensitive search for coupon code
            return await _context.Coupons
                .FirstOrDefaultAsync(c => c.Code.ToUpper() == code.ToUpper());
        }

        public async Task<Coupon> AddCouponAsync(Coupon coupon)
        {
            // Convert code to uppercase for case-insensitive storage
            coupon.Code = coupon.Code.ToUpper();
            coupon.CreatedAt = DateTime.Now;
            
            _context.Coupons.Add(coupon);
            await _context.SaveChangesAsync();
            return coupon;
        }

        public async Task<Coupon> UpdateCouponAsync(Coupon coupon)
        {
            // Convert code to uppercase for case-insensitive storage
            coupon.Code = coupon.Code.ToUpper();
            coupon.UpdatedAt = DateTime.Now;
            
            _context.Coupons.Update(coupon);
            await _context.SaveChangesAsync();
            return coupon;
        }

        public async Task DeleteCouponAsync(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon != null)
            {
                _context.Coupons.Remove(coupon);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> CouponExistsAsync(int id)
        {
            return await _context.Coupons.AnyAsync(c => c.Id == id);
        }

        public async Task<bool> CouponCodeExistsAsync(string code, int excludeId = 0)
        {
            // Case-insensitive check for duplicate coupon codes
            return await _context.Coupons
                .AnyAsync(c => c.Code.ToUpper() == code.ToUpper() && c.Id != excludeId);
        }
    }
}

