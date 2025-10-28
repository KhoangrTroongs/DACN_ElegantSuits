using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NgoHuuDuc_2280600725.Models
{
    public class Coupon
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã giảm giá không được để trống")]
        [StringLength(50, ErrorMessage = "Mã giảm giá không được vượt quá 50 ký tự")]
        [Display(Name = "Mã giảm giá")]
        public string Code { get; set; } = "";

        [Required(ErrorMessage = "Số lượng không được để trống")]
        [Display(Name = "Số lượng")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Phần trăm giảm giá không được để trống")]
        [Range(0, 100, ErrorMessage = "Phần trăm giảm giá phải từ 0 đến 100")]
        [Display(Name = "Phần trăm giảm giá")]
        [Column(TypeName = "decimal(5, 2)")]
        public decimal DiscountPercentage { get; set; }

        [Required(ErrorMessage = "Ngày hết hạn không được để trống")]
        [Display(Name = "Ngày hết hạn")]
        [DataType(DataType.DateTime)]
        public DateTime ExpiryDate { get; set; }

        [Display(Name = "Kích hoạt")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "Tối thiểu")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal MinimumAmount { get; set; } = 0;

        [Display(Name = "Biên lợi nhuận")]
        [Column(TypeName = "decimal(5, 2)")]
        public decimal ProfitMargin { get; set; } = 0.45m; // 45% mặc định
    }
}

