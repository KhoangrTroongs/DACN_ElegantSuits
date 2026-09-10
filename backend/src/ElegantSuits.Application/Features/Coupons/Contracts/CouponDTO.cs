namespace ElegantSuits.Application.Features.Coupons.Contracts;

public class CouponDTO
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string? Description { get; set; }
    public int Quantity { get; set; }
    public int DiscountPercentage { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public decimal MinimumAmount { get; set; }
    public bool IsExpired => DateTime.Now > ExpiryDate;
    public bool IsDepleted => Quantity == 0;
    public string Status => !IsActive ? "Không kích hoạt" : IsExpired ? "Đã hết hạn" : IsDepleted ? "Đã hết số lượng" : "Còn hiệu lực";
}

public class CreateCouponDTO
{
    public string Code { get; set; } = "";
    public string? Description { get; set; }
    public int Quantity { get; set; }
    public int DiscountPercentage { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal MinimumAmount { get; set; } = 0;
}

public class UpdateCouponDTO
{
    public string Code { get; set; } = "";
    public string? Description { get; set; }
    public int Quantity { get; set; }
    public int DiscountPercentage { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsActive { get; set; }
    public decimal MinimumAmount { get; set; }
}

public class ValidateCouponRequest
{
    public string Code { get; set; } = "";
    public decimal OrderAmount { get; set; }
}

public class ValidateCouponResult
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = "";
    public int DiscountPercentage { get; set; }
    public decimal DiscountAmount { get; set; }
}
