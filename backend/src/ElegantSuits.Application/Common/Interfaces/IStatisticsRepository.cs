using ElegantSuits.Application.Features.Statistics.Contracts;

namespace ElegantSuits.Application.Common.Interfaces;

public interface IStatisticsRepository
{
    Task<StatisticsOverviewDTO> GetOverviewStatisticsAsync(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);
    Task<List<DailyRevenueDTO>> GetDailyRevenueAsync(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);
}
