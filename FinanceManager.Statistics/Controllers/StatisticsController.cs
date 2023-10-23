using FinanceManager.Statistics.Models;
using FinanceManager.Statistics.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.Statistics.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatisticsController : ControllerBase
{
    private readonly IBalanceLevelStatisticsService _balanceLevelStatisticsService;
    private readonly ICategoryTotalTimeStatisticsService _categoryTotalTimeStatisticsService;

    public StatisticsController(
        IBalanceLevelStatisticsService balanceLevelStatisticsService,
        ICategoryTotalTimeStatisticsService categoryTotalTimeStatisticsService)
    {
        _balanceLevelStatisticsService = balanceLevelStatisticsService;
        _categoryTotalTimeStatisticsService = categoryTotalTimeStatisticsService;
    }

    [HttpGet("account/{accountId}/balance")]
    public async Task<IActionResult> GetAccountBalanceStatistics([FromRoute] Guid accountId)
    {
        return Ok(await _balanceLevelStatisticsService.GetAsync(accountId));
    }

    [HttpGet("account/{accountId}/categoty{categoryId}")]
    public async Task<IActionResult> GetCategoryStatistics(
        [FromRoute] Guid accountId,
        [FromRoute] Guid categoryId,
        [FromQuery] CategoryStatisticsRequest request)
    {
        return Ok(await _categoryTotalTimeStatisticsService.GetAsync(new CategoryStatisticsParameters
        {
            AccountId = accountId,
            CategoryId = categoryId,
            TimeType = request.TimeType,
            Year = request.Year
        }));
    }
}