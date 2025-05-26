using System.ComponentModel.DataAnnotations;

namespace NgoHuuDuc_2280600725.Models.Enums
{
    public enum PaymentMethod
    {
        [Display(Name = "Thanh toán khi nhận hàng (COD)")]
        CashOnDelivery = 0,
        
        [Display(Name = "Thanh toán online")]
        OnlinePayment = 1
    }
}
