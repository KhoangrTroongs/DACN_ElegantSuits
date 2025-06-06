using System.ComponentModel.DataAnnotations;

namespace NgoHuuDuc_2280600725.DTOs
{
    /// <summary>
    /// DTO cho thống kê tổng quan hệ thống
    /// </summary>
    public class StatisticsOverviewDTO
    {
        /// <summary>
        /// Khoảng thời gian thống kê
        /// </summary>
        public DateRangeDTO DateRange { get; set; } = new();

        /// <summary>
        /// Tổng số đơn hàng trong khoảng thời gian
        /// </summary>
        public int TotalOrders { get; set; }

        /// <summary>
        /// Tổng doanh thu (chỉ tính từ đơn hàng đã giao)
        /// </summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>
        /// Tổng số sản phẩm trong hệ thống
        /// </summary>
        public int TotalProducts { get; set; }

        /// <summary>
        /// Tổng số người dùng đã đăng ký
        /// </summary>
        public int TotalUsers { get; set; }

        /// <summary>
        /// Thống kê số lượng đơn hàng theo từng trạng thái
        /// </summary>
        public OrdersByStatusDTO OrdersByStatus { get; set; } = new();

        /// <summary>
        /// Thống kê doanh thu theo từng trạng thái đơn hàng
        /// </summary>
        public RevenueByStatusDTO RevenueByStatus { get; set; } = new();
    }

    /// <summary>
    /// DTO cho khoảng thời gian thống kê
    /// </summary>
    public class DateRangeDTO
    {
        /// <summary>
        /// Ngày bắt đầu thống kê
        /// </summary>
        [Required]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Ngày kết thúc thống kê
        /// </summary>
        [Required]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Số ngày trong khoảng thời gian
        /// </summary>
        public int TotalDays => (EndDate - StartDate).Days + 1;
    }

    /// <summary>
    /// DTO cho thống kê số lượng đơn hàng theo trạng thái
    /// </summary>
    public class OrdersByStatusDTO
    {
        /// <summary>
        /// Số đơn hàng chờ xác nhận
        /// </summary>
        public int Pending { get; set; }

        /// <summary>
        /// Số đơn hàng đã xác nhận
        /// </summary>
        public int Confirmed { get; set; }

        /// <summary>
        /// Số đơn hàng đang giao
        /// </summary>
        public int Shipping { get; set; }

        /// <summary>
        /// Số đơn hàng đã giao thành công
        /// </summary>
        public int Delivered { get; set; }

        /// <summary>
        /// Số đơn hàng đã bị hủy
        /// </summary>
        public int Cancelled { get; set; }

        /// <summary>
        /// Số đơn hàng đã hoàn trả
        /// </summary>
        public int Returned { get; set; }

        /// <summary>
        /// Tổng số đơn hàng
        /// </summary>
        public int Total => Pending + Confirmed + Shipping + Delivered + Cancelled + Returned;
    }

    /// <summary>
    /// DTO cho thống kê doanh thu theo trạng thái đơn hàng
    /// </summary>
    public class RevenueByStatusDTO
    {
        /// <summary>
        /// Doanh thu từ đơn hàng chờ xác nhận
        /// </summary>
        public decimal Pending { get; set; }

        /// <summary>
        /// Doanh thu từ đơn hàng đã xác nhận
        /// </summary>
        public decimal Confirmed { get; set; }

        /// <summary>
        /// Doanh thu từ đơn hàng đang giao
        /// </summary>
        public decimal Shipping { get; set; }

        /// <summary>
        /// Doanh thu từ đơn hàng đã giao (doanh thu thực tế)
        /// </summary>
        public decimal Delivered { get; set; }

        /// <summary>
        /// Doanh thu từ đơn hàng đã hủy
        /// </summary>
        public decimal Cancelled { get; set; }

        /// <summary>
        /// Doanh thu từ đơn hàng hoàn trả
        /// </summary>
        public decimal Returned { get; set; }

        /// <summary>
        /// Tổng doanh thu tiềm năng (tất cả trạng thái)
        /// </summary>
        public decimal TotalPotential => Pending + Confirmed + Shipping + Delivered + Cancelled + Returned;

        /// <summary>
        /// Doanh thu thực tế (chỉ từ đơn hàng đã giao)
        /// </summary>
        public decimal ActualRevenue => Delivered;
    }

    /// <summary>
    /// DTO cho thống kê doanh thu theo ngày
    /// </summary>
    public class DailyRevenueDTO
    {
        /// <summary>
        /// Ngày thống kê
        /// </summary>
        [Required]
        public DateTime Date { get; set; }

        /// <summary>
        /// Số đơn hàng trong ngày
        /// </summary>
        public int OrderCount { get; set; }

        /// <summary>
        /// Doanh thu trong ngày (chỉ tính từ đơn hàng đã giao)
        /// </summary>
        public decimal Revenue { get; set; }

        /// <summary>
        /// Doanh thu trung bình mỗi đơn hàng
        /// </summary>
        public decimal AverageOrderValue => OrderCount > 0 ? Revenue / OrderCount : 0;

        /// <summary>
        /// Ngày được format theo định dạng dd/MM/yyyy
        /// </summary>
        public string FormattedDate => Date.ToString("dd/MM/yyyy");

        /// <summary>
        /// Ngày được format ngắn gọn dd/MM
        /// </summary>
        public string ShortFormattedDate => Date.ToString("dd/MM");
    }

    /// <summary>
    /// DTO cho thông tin sản phẩm bán chạy
    /// </summary>
    public class TopProductDTO
    {
        /// <summary>
        /// ID của sản phẩm
        /// </summary>
        [Required]
        public int ProductId { get; set; }

        /// <summary>
        /// Tên sản phẩm
        /// </summary>
        [Required]
        public string ProductName { get; set; } = "";

        /// <summary>
        /// URL hình ảnh sản phẩm
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Tổng số lượng đã bán (chỉ tính từ đơn hàng đã giao)
        /// </summary>
        public int QuantitySold { get; set; }

        /// <summary>
        /// Tổng doanh thu từ sản phẩm này
        /// </summary>
        public decimal Revenue { get; set; }

        /// <summary>
        /// Giá trung bình mỗi sản phẩm
        /// </summary>
        public decimal AveragePrice => QuantitySold > 0 ? Revenue / QuantitySold : 0;

        /// <summary>
        /// Thứ hạng trong danh sách top sản phẩm
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// Phần trăm đóng góp vào tổng doanh thu
        /// </summary>
        public decimal RevenuePercentage { get; set; }

        /// <summary>
        /// URL hình ảnh với fallback
        /// </summary>
        public string DisplayImageUrl => !string.IsNullOrEmpty(ImageUrl) ? ImageUrl : "/images/no-image.png";

        /// <summary>
        /// Tên sản phẩm được rút gọn (tối đa 50 ký tự)
        /// </summary>
        public string ShortProductName => ProductName.Length > 50 
            ? ProductName.Substring(0, 47) + "..." 
            : ProductName;
    }

    /// <summary>
    /// DTO cho response danh sách top sản phẩm
    /// </summary>
    public class TopProductsResponseDTO
    {
        /// <summary>
        /// Danh sách top sản phẩm
        /// </summary>
        public List<TopProductDTO> Products { get; set; } = new();

        /// <summary>
        /// Tổng doanh thu của tất cả sản phẩm trong khoảng thời gian
        /// </summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>
        /// Tổng số lượng sản phẩm đã bán
        /// </summary>
        public int TotalQuantitySold { get; set; }

        /// <summary>
        /// Khoảng thời gian thống kê
        /// </summary>
        public DateRangeDTO DateRange { get; set; } = new();

        /// <summary>
        /// Số lượng sản phẩm được yêu cầu
        /// </summary>
        public int RequestedLimit { get; set; }

        /// <summary>
        /// Số lượng sản phẩm thực tế trả về
        /// </summary>
        public int ActualCount => Products.Count;
    }
}
