using ElegantSuits.Domain.Enums;

namespace ElegantSuits.Application.Features.Orders.Contracts;

public class OrderDTO
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalPrice { get; set; }
    public string ShippingAddress { get; set; } = "";
    public string? Notes { get; set; }
    public OrderStatus Status { get; set; }
    public string PaymentMethod { get; set; } = "COD";
    public PaymentStatus PaymentStatus { get; set; }
    public List<OrderDetailDTO> OrderDetails { get; set; } = new List<OrderDetailDTO>();
}

public class OrderDetailDTO
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? Size { get; set; }
    public string? ProductImageUrl { get; set; }
}

public class CreateOrderDTO
{
    public string ShippingAddress { get; set; } = "";
    public string? Notes { get; set; }
    public string PaymentMethod { get; set; } = "COD";
    public string? CouponCode { get; set; }
}

public class UpdateOrderStatusDTO
{
    public OrderStatus Status { get; set; }
}
