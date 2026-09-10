using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Features.Statistics.Contracts;
using ElegantSuits.Domain.Entities;
using ElegantSuits.Domain.Enums;
using ElegantSuits.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ElegantSuits.Infrastructure.Repositories;

public class StatisticsRepository : IStatisticsRepository
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public StatisticsRepository(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<StatisticsOverviewDTO> GetOverviewStatisticsAsync(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
    {
        var start = startDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var end = (endDate ?? DateTime.Now).Date.AddDays(1).AddTicks(-1);

        var orders = await _context.Orders
            .AsNoTracking()
            .Where(o => o.OrderDate >= start && o.OrderDate <= end)
            .ToListAsync(cancellationToken);

        return new StatisticsOverviewDTO
        {
            DateRange = new DateRangeDTO
            {
                StartDate = start,
                EndDate = end
            },
            TotalOrders = orders.Count,
            TotalRevenue = orders.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.TotalPrice),
            TotalProducts = await _context.Products.CountAsync(cancellationToken),
            TotalUsers = await _userManager.Users.CountAsync(cancellationToken),
            OrdersByStatus = new OrdersByStatusDTO
            {
                Pending = orders.Count(o => o.Status == OrderStatus.Pending),
                Confirmed = orders.Count(o => o.Status == OrderStatus.Confirmed),
                Shipping = orders.Count(o => o.Status == OrderStatus.Shipping),
                Delivered = orders.Count(o => o.Status == OrderStatus.Delivered),
                Cancelled = orders.Count(o => o.Status == OrderStatus.Cancelled),
                Returned = 0
            },
            RevenueByStatus = new RevenueByStatusDTO
            {
                Pending = orders.Where(o => o.Status == OrderStatus.Pending).Sum(o => o.TotalPrice),
                Confirmed = orders.Where(o => o.Status == OrderStatus.Confirmed).Sum(o => o.TotalPrice),
                Shipping = orders.Where(o => o.Status == OrderStatus.Shipping).Sum(o => o.TotalPrice),
                Delivered = orders.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.TotalPrice),
                Cancelled = orders.Where(o => o.Status == OrderStatus.Cancelled).Sum(o => o.TotalPrice),
                Returned = 0
            }
        };
    }

    public async Task<List<DailyRevenueDTO>> GetDailyRevenueAsync(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
    {
        var start = startDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var end = (endDate ?? DateTime.Now).Date.AddDays(1).AddTicks(-1);

        var orders = await _context.Orders
            .AsNoTracking()
            .Where(o => o.OrderDate >= start && o.OrderDate <= end)
            .ToListAsync(cancellationToken);

        return orders
            .GroupBy(o => o.OrderDate.Date)
            .Select(g => new DailyRevenueDTO
            {
                Date = g.Key,
                Revenue = g.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.TotalPrice),
                OrderCount = g.Count()
            })
            .OrderBy(d => d.Date)
            .ToList();
    }
}
