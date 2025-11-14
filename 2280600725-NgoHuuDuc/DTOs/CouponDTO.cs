using System.ComponentModel.DataAnnotations;

namespace NgoHuuDuc_2280600725.DTOs
{
    public class CouponDTO
    {
        public int Id { get; set; }

        [Display(Name = "Mã giảm giá")]
        public string Code { get; set; } = "";

        [Display(Name = "Chú thích")]
        public string? Description { get; set; }

        [Display(Name = "Số lượng")]
        public int Quantity { get; set; }

        [Display(Name = "Phần trăm giảm giá")]
        public int DiscountPercentage { get; set; }

        [Display(Name = "Ngày hết hạn")]
        public DateTime ExpiryDate { get; set; }

        [Display(Name = "Kích hoạt")]
        public bool IsActive { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "Tối thiểu")]
        public decimal MinimumAmount { get; set; }

        // Computed properties for display
        public bool IsExpired => DateTime.Now > ExpiryDate;
        public bool IsDepleted => Quantity == 0;
        public string Status => !IsActive ? "Không kích hoạt" : IsExpired ? "Đã hết hạn" : IsDepleted ? "Đã hết số lượng" : "Còn hiệu lực";
        public int UsagePercentage => Quantity == -1 ? 0 : 100; // Simplified for display
    }

    public class CreateCouponDTO
    {
        [StringLength(50, ErrorMessage = "Mã giảm giá không được vượt quá 50 ký tự")]
        [Display(Name = "Mã giảm giá")]
        public string Code { get; set; } = "";

        [StringLength(500, ErrorMessage = "Chú thích không được vượt quá 500 ký tự")]
        [Display(Name = "Chú thích")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Số lượng không được để trống")]
        [CustomValidation(typeof(CouponDTOValidator), nameof(CouponDTOValidator.ValidateQuantity))]
        [Display(Name = "Số lượng")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Phần trăm giảm giá không được để trống")]
        [Range(1, 100, ErrorMessage = "Phần trăm giảm giá phải từ 1 đến 100")]
        [Display(Name = "Phần trăm giảm giá")]
        public int DiscountPercentage { get; set; }

        [Required(ErrorMessage = "Giá tối thiểu không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá tối thiểu phải lớn hơn hoặc bằng 0")]
        [Display(Name = "Giá tối thiểu")]
        public decimal MinimumAmount { get; set; } = 0;

        [Display(Name = "Ngày hết hạn")]
        [DataType(DataType.DateTime)]
        public DateTime? ExpiryDate { get; set; }

        [Display(Name = "Kích hoạt")]
        public bool IsActive { get; set; } = true;
    }

    public class UpdateCouponDTO
    {
        [StringLength(500, ErrorMessage = "Chú thích không được vượt quá 500 ký tự")]
        [Display(Name = "Chú thích")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Số lượng không được để trống")]
        [CustomValidation(typeof(CouponDTOValidator), nameof(CouponDTOValidator.ValidateQuantity))]
        [Display(Name = "Số lượng")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Phần trăm giảm giá không được để trống")]
        [Range(1, 100, ErrorMessage = "Phần trăm giảm giá phải từ 1 đến 100")]
        [Display(Name = "Phần trăm giảm giá")]
        public int DiscountPercentage { get; set; }

        [Required(ErrorMessage = "Giá tối thiểu không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá tối thiểu phải lớn hơn hoặc bằng 0")]
        [Display(Name = "Giá tối thiểu")]
        public decimal MinimumAmount { get; set; } = 0;

        [Display(Name = "Ngày hết hạn")]
        [DataType(DataType.DateTime)]
        public DateTime? ExpiryDate { get; set; }

        [Display(Name = "Kích hoạt")]
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Validator for Coupon DTOs
    /// </summary>
    public static class CouponDTOValidator
    {
        public static ValidationResult ValidateQuantity(int quantity, ValidationContext _)
        {
            // Quantity must be either -1 (unlimited) or > 0 (limited quantity)
            if (quantity == -1 || quantity > 0)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult("Số lượng phải là -1 (không giới hạn) hoặc lớn hơn 0");
        }
    }
}

