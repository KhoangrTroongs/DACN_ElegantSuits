using NgoHuuDuc_2280600725.DTOs;
using NgoHuuDuc_2280600725.Models;
using NgoHuuDuc_2280600725.Responsitories;
using NgoHuuDuc_2280600725.Services.Interfaces;

namespace NgoHuuDuc_2280600725.Services
{
    public class CouponService : ICouponService
    {
        private readonly ICouponRepository _couponRepository;
        private readonly IProductRepository _productRepository;
        private readonly ILogger<CouponService> _logger;

        public CouponService(
            ICouponRepository couponRepository,
            IProductRepository productRepository,
            ILogger<CouponService> logger)
        {
            _couponRepository = couponRepository;
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<CouponDTO>> GetAllCouponsAsync()
        {
            var coupons = await _couponRepository.GetAllCouponsAsync();
            return coupons.Select(MapToCouponDTO);
        }

        public async Task<CouponDTO?> GetCouponByIdAsync(int id)
        {
            var coupon = await _couponRepository.GetCouponByIdAsync(id);
            return coupon != null ? MapToCouponDTO(coupon) : null;
        }

        public async Task<CouponDTO?> GetCouponByCodeAsync(string code)
        {
            var coupon = await _couponRepository.GetCouponByCodeAsync(code);
            return coupon != null ? MapToCouponDTO(coupon) : null;
        }

        public async Task<CouponDTO> AddCouponAsync(CreateCouponDTO couponDto)
        {
            // Check if coupon code already exists
            if (await _couponRepository.CouponCodeExistsAsync(couponDto.Code))
            {
                throw new InvalidOperationException("Mã giảm giá này đã tồn tại.");
            }

            var coupon = new Coupon
            {
                Code = couponDto.Code.ToUpper(),
                Quantity = couponDto.Quantity,
                DiscountPercentage = couponDto.DiscountPercentage,
                ExpiryDate = couponDto.ExpiryDate ?? DateTime.MaxValue, // Use MaxValue if no expiry date (unlimited)
                IsActive = couponDto.IsActive,
                CreatedAt = DateTime.Now
            };

            var createdCoupon = await _couponRepository.AddCouponAsync(coupon);
            return MapToCouponDTO(createdCoupon);
        }

        public async Task<CouponDTO?> UpdateCouponAsync(int id, UpdateCouponDTO couponDto)
        {
            var coupon = await _couponRepository.GetCouponByIdAsync(id);
            if (coupon == null)
            {
                return null;
            }

            coupon.Quantity = couponDto.Quantity;
            coupon.DiscountPercentage = couponDto.DiscountPercentage;
            coupon.ExpiryDate = couponDto.ExpiryDate ?? DateTime.MaxValue; // Use MaxValue if no expiry date (unlimited)
            coupon.IsActive = couponDto.IsActive;
            coupon.UpdatedAt = DateTime.Now;

            var updatedCoupon = await _couponRepository.UpdateCouponAsync(coupon);
            return MapToCouponDTO(updatedCoupon);
        }

        public async Task<bool> DeleteCouponAsync(int id)
        {
            var coupon = await _couponRepository.GetCouponByIdAsync(id);
            if (coupon == null)
            {
                return false;
            }

            await _couponRepository.DeleteCouponAsync(id);
            return true;
        }

        public async Task<bool> CouponExistsAsync(int id)
        {
            return await _couponRepository.CouponExistsAsync(id);
        }

        public async Task<bool> CouponCodeExistsAsync(string code, int excludeId = 0)
        {
            return await _couponRepository.CouponCodeExistsAsync(code, excludeId);
        }

        public async Task<CouponValidationResult> ValidateCouponAsync(string code, decimal cartTotal = 0)
        {
            // Get coupon by code (case-insensitive)
            var coupon = await _couponRepository.GetCouponByCodeAsync(code);

            if (coupon == null)
            {
                return new CouponValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Mã giảm giá không tồn tại"
                };
            }

            // Check if coupon is active
            if (!coupon.IsActive)
            {
                return new CouponValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Mã giảm giá không khả dụng"
                };
            }

            // Check if coupon has expired
            if (DateTime.Now > coupon.ExpiryDate)
            {
                return new CouponValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Mã giảm giá đã hết hạn"
                };
            }

            // Check if coupon quantity is available (0 means depleted, -1 means unlimited)
            if (coupon.Quantity == 0)
            {
                return new CouponValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Mã giảm giá đã hết số lượng"
                };
            }

            // Check if cart total meets minimum amount (if cartTotal is provided)
            if (cartTotal > 0 && cartTotal < coupon.MinimumAmount)
            {
                var remainingAmount = coupon.MinimumAmount - cartTotal;
                return new CouponValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"Mua thêm {remainingAmount:N0}đ để được giảm {coupon.DiscountPercentage}%"
                };
            }

            // Coupon is valid
            return new CouponValidationResult
            {
                IsValid = true,
                Coupon = MapToCouponDTO(coupon)
            };
        }

        public async Task<bool> DecrementCouponQuantityAsync(string code)
        {
            var coupon = await _couponRepository.GetCouponByCodeAsync(code);
            if (coupon == null)
            {
                return false;
            }

            // Only decrement if quantity is not unlimited (-1)
            if (coupon.Quantity != -1)
            {
                coupon.Quantity--;
            }

            await _couponRepository.UpdateCouponAsync(coupon);
            return true;
        }

        public async Task<string> GenerateUniqueCouponCodeAsync()
        {
            string code;
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            const int codeLength = 8;
            var random = new Random();

            // Keep generating until we find a unique code
            do
            {
                var codeBuilder = new System.Text.StringBuilder();
                for (int i = 0; i < codeLength; i++)
                {
                    codeBuilder.Append(chars[random.Next(chars.Length)]);
                }
                code = $"COUPON-{codeBuilder}";
            } while (await _couponRepository.CouponCodeExistsAsync(code));

            return code;
        }

        public async Task<List<CouponDTO>> GetAvailableCouponsAsync(decimal cartTotal, bool hasFreeship, List<int> productIds)
        {
            try
            {
                var availableCoupons = new List<CouponDTO>();

                // Lấy thông tin sản phẩm
                var products = await _productRepository.GetProductsByIdsAsync(productIds);

                // Bước 2: Kiểm tra margin sản phẩm (≥ 30%)
                if (products.Any(p => p.ProfitMargin < 0.30m))
                {
                    _logger.LogInformation("Product margin < 30%, no coupons available");
                    return availableCoupons; // Trả về danh sách trống
                }

                // Lấy tất cả coupon từ database
                var allCoupons = await _couponRepository.GetAllCouponsAsync();

                foreach (var coupon in allCoupons)
                {
                    // Bước 1: Lọc coupon cơ bản
                    if (coupon.ExpiryDate <= DateTime.Now ||
                        coupon.Quantity == 0 ||
                        !coupon.IsActive ||
                        cartTotal < coupon.MinimumAmount)
                    {
                        continue;
                    }

                    // Bước 3: Tính lợi nhuận sau giảm
                    var profitAfter = (cartTotal * 0.45m) - (cartTotal * (coupon.DiscountPercentage / 100));
                    var profitRatio = profitAfter / cartTotal;

                    if (profitRatio < 0.20m)
                    {
                        continue;
                    }

                    // Bước 4: Xử lý freeship
                    if (hasFreeship && coupon.DiscountPercentage > 15)
                    {
                        continue;
                    }

                    availableCoupons.Add(MapToCouponDTO(coupon));
                }

                // Bước 5: Chọn coupon có % cao nhất
                if (availableCoupons.Any())
                {
                    var bestCoupon = availableCoupons.OrderByDescending(c => c.DiscountPercentage).First();
                    _logger.LogInformation($"Best coupon selected: {bestCoupon.Code} ({bestCoupon.DiscountPercentage}%)");
                    return new List<CouponDTO> { bestCoupon };
                }

                return availableCoupons;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available coupons");
                return new List<CouponDTO>();
            }
        }

        private CouponDTO MapToCouponDTO(Coupon coupon)
        {
            return new CouponDTO
            {
                Id = coupon.Id,
                Code = coupon.Code,
                Quantity = coupon.Quantity,
                DiscountPercentage = coupon.DiscountPercentage,
                ExpiryDate = coupon.ExpiryDate,
                IsActive = coupon.IsActive,
                CreatedAt = coupon.CreatedAt,
                UpdatedAt = coupon.UpdatedAt
            };
        }
    }
}

