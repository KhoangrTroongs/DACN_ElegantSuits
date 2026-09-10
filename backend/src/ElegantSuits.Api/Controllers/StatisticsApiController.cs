using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Common.Models;
using ElegantSuits.Application.Features.Statistics.Contracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElegantSuits.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
public class StatisticsApiController : ControllerBase
{
    private readonly IStatisticsRepository _statisticsRepository;

    public StatisticsApiController(IStatisticsRepository statisticsRepository)
    {
        _statisticsRepository = statisticsRepository;
    }

    // GET: api/StatisticsApi/overview
    [HttpGet("overview")]
    public async Task<ActionResult<StatisticsOverviewDTO>> GetOverviewStatistics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _statisticsRepository.GetOverviewStatisticsAsync(startDate, endDate, cancellationToken);
        return Ok(result);
    }

    // GET: api/StatisticsApi/daily-revenue
    [HttpGet("daily-revenue")]
    public async Task<ActionResult<List<DailyRevenueDTO>>> GetDailyRevenue(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _statisticsRepository.GetDailyRevenueAsync(startDate, endDate, cancellationToken);
        return Ok(result);
    }
}
