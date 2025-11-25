using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using NgoHuuDuc_2280600725.Models.Enums;

namespace NgoHuuDuc_2280600725.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public decimal TotalPrice { get; set; }

        [Required(ErrorMessage = "Địa chỉ giao hàng không được để trống")]
        [Display(Name = "Địa chỉ giao hàng")]
        public string ShippingAddress { get; set; } = "";

        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }

        [Display(Name = "Trạng thái")]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Display(Name = "Mã giảm giá")]
        public string? CouponCode { get; set; }

        [Display(Name = "Số tiền giảm")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Display(Name = "Phương thức thanh toán")]
        public string PaymentMethod { get; set; } = "COD"; // COD, MoMo

        [Display(Name = "Trạng thái thanh toán")]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        [Display(Name = "Mã giao dịch")]
        public string? TransactionId { get; set; }

        [Display(Name = "Tổng tiền")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Trạng thái đơn hàng")]
        public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;

        [ForeignKey("UserId")]
        [ValidateNever]
        public ApplicationUser? User { get; set; }
        public List<OrderDetail>? OrderDetails { get; set; }
    }
}
