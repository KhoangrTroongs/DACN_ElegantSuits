using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ElegantSuits.Web.Models;

namespace ElegantSuits.Web.Controllers;

[Authorize(Roles = "Administrator")]
public class StatisticsController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<StatisticsController> _logger;

    public StatisticsController(
        HttpClient httpClient,
        IConfiguration config,
        ILogger<StatisticsController> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
    }

    public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
    {
        if (!startDate.HasValue)
        {
            startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        }
        if (!endDate.HasValue)
        {
            endDate = DateTime.Now;
        }

        ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
        ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");

        var vm = new StatisticsViewModel
        {
            TotalOrders = 0,
            TotalRevenue = 0,
            TotalProducts = 0,
            TotalUsers = 0
        };

        try
        {
            var baseUrl = _config["BackendApi:BaseUrl"] ?? "http://localhost:5097";
            var token = HttpContext.Session.GetString("JwtToken");
            var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/api/StatisticsApi/overview");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var res = await _httpClient.SendAsync(request);
            if (res.IsSuccessStatusCode)
            {
                var content = await res.Content.ReadFromJsonAsync<StatisticsOverviewResponse>();
                if (content != null)
                {
                    vm.TotalOrders = content.TotalOrders;
                    vm.TotalRevenue = content.TotalRevenue;
                    vm.TotalProducts = content.TotalProducts;
                    vm.TotalUsers = content.TotalUsers;

                    if (content.OrdersByStatus != null)
                    {
                        vm.PendingOrders = content.OrdersByStatus.Pending;
                        vm.ConfirmedOrders = content.OrdersByStatus.Confirmed;
                        vm.ShippingOrders = content.OrdersByStatus.Shipping;
                        vm.DeliveredOrders = content.OrdersByStatus.Delivered;
                        vm.CancelledOrders = content.OrdersByStatus.Cancelled;
                        vm.ReturnedOrders = content.OrdersByStatus.Returned;
                    }

                    if (content.RevenueByStatus != null)
                    {
                        vm.PendingRevenue = content.RevenueByStatus.Pending;
                        vm.ConfirmedRevenue = content.RevenueByStatus.Confirmed;
                        vm.ShippingRevenue = content.RevenueByStatus.Shipping;
                        vm.DeliveredRevenue = content.RevenueByStatus.Delivered;
                        vm.CancelledRevenue = content.RevenueByStatus.Cancelled;
                        vm.ReturnedRevenue = content.RevenueByStatus.Returned;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting statistics overview");
        }

        return View(vm);
    }

    // Maps directly to StatisticsOverviewDTO from BE
    public class StatisticsOverviewResponse
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalProducts { get; set; }
        public int TotalUsers { get; set; }
        public OrdersByStatusData? OrdersByStatus { get; set; }
        public RevenueByStatusData? RevenueByStatus { get; set; }
    }

    public class OrdersByStatusData
    {
        public int Pending { get; set; }
        public int Confirmed { get; set; }
        public int Shipping { get; set; }
        public int Delivered { get; set; }
        public int Cancelled { get; set; }
        public int Returned { get; set; }
    }

    public class RevenueByStatusData
    {
        public decimal Pending { get; set; }
        public decimal Confirmed { get; set; }
        public decimal Shipping { get; set; }
        public decimal Delivered { get; set; }
        public decimal Cancelled { get; set; }
        public decimal Returned { get; set; }
    }
}
