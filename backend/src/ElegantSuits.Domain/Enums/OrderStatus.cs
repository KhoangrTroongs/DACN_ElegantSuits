namespace ElegantSuits.Domain.Enums;

public enum OrderStatus
{
    Pending = 0,
    Processing = 1,
    Confirmed = 2,
    Shipping = 3,
    Delivered = 4,
    Completed = 5,
    Cancelled = 6,
    Returned = 7
}

public static class OrderStatusExtensions
{
    public static string GetDisplayName(this OrderStatus status) => status switch
    {
        OrderStatus.Pending => "Chờ xử lý",
        OrderStatus.Processing => "Đang xử lý",
        OrderStatus.Confirmed => "Đã xác nhận",
        OrderStatus.Shipping => "Đang giao hàng",
        OrderStatus.Delivered => "Đã giao hàng",
        OrderStatus.Completed => "Hoàn thành",
        OrderStatus.Cancelled => "Đã hủy",
        OrderStatus.Returned => "Hoàn trả",
        _ => status.ToString()
    };
}
