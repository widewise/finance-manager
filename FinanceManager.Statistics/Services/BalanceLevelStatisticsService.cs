using FinanceManager.Statistics.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.Statistics.Services;

public interface IBalanceLevelStatisticsService
{
    Task<BalanceStatisticsModel[]> GetAsync(Guid accountId);
    Task UpdateStatisticsAsync(Guid accountId, decimal balance, DateTime date);
}

public class BalanceLevelStatisticsService: IBalanceLevelStatisticsService
{
    private readonly ILogger<BalanceLevelStatisticsService> _logger;
    private readonly AppDbContext _appDbContext;

    public BalanceLevelStatisticsService(
        ILogger<BalanceLevelStatisticsService> logger,
        AppDbContext appDbContext)
    {
        _logger = logger;
        _appDbContext = appDbContext;
    }

    public async Task<BalanceStatisticsModel[]> GetAsync(Guid accountId)
    {
        return await _appDbContext.BalanceLevelStatistics
            .Where(x => x.AccountId == accountId)
            .Select(x => new BalanceStatisticsModel
            {
                Value = x.Balance,
                Date = x.Date
            })
            .ToArrayAsync();
    }

    public async Task UpdateStatisticsAsync(Guid accountId, decimal balance, DateTime date)
    {
        try
        {
            var statistics = await _appDbContext.BalanceLevelStatistics.FirstOrDefaultAsync(
                x => x.AccountId == accountId && x.Date == date);
            if (statistics != null)
            {
                statistics.Balance += balance;
                _appDbContext.BalanceLevelStatistics.Update(statistics);
            }
            else
            {
                await _appDbContext.BalanceLevelStatistics.AddAsync(new BalanceLevelStatistics
                {
                    Id = Guid.NewGuid(),
                    AccountId = accountId,
                    Balance = balance,
                    Date = date
                });
            }

            await _appDbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, 
                "Update balance level statistics for account with id {AccountId} error: {ErrorMessage}",
                accountId,
                e.Message);
        }
    }
}