namespace ElegantSuits.Application.Features.Statistics.Contracts;

public class StatisticsOverviewDTO
{
    public DateRangeDTO DateRange { get; set; } = new();
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalProducts { get; set; }
    public int TotalUsers { get; set; }
    public OrdersByStatusDTO OrdersByStatus { get; set; } = new();
    public RevenueByStatusDTO RevenueByStatus { get; set; } = new();
}

public class DateRangeDTO
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class OrdersByStatusDTO
{
    public int Pending { get; set; }
    public int Confirmed { get; set; }
    public int Shipping { get; set; }
    public int Delivered { get; set; }
    public int Cancelled { get; set; }
    public int Returned { get; set; }
}

public class RevenueByStatusDTO
{
    public decimal Pending { get; set; }
    public decimal Confirmed { get; set; }
    public decimal Shipping { get; set; }
    public decimal Delivered { get; set; }
    public decimal Cancelled { get; set; }
    public decimal Returned { get; set; }
}

public class DailyRevenueDTO
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}

public class TopSellingProductDTO
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public int TotalQuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
}
