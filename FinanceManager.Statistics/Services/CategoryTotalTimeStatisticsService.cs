using FinanceManager.Statistics.Extensions;
using FinanceManager.Statistics.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.Statistics.Services;

public interface ICategoryTotalTimeStatisticsService
{
    Task<CategoryStatisticsModel[]> GetAsync(CategoryStatisticsParameters parameters);
    Task UpdateStatisticsAsync(Guid accountId, Guid categoryId, decimal value, DateTime date);
}

public class CategoryTotalTimeStatisticsService : ICategoryTotalTimeStatisticsService
{
    private readonly ILogger<CategoryTotalTimeStatisticsService> _logger;
    private readonly AppDbContext _appDbContext;

    public CategoryTotalTimeStatisticsService(
        ILogger<CategoryTotalTimeStatisticsService> logger,
        AppDbContext appDbContext)
    {
        _logger = logger;
        _appDbContext = appDbContext;
    }

    public async Task<CategoryStatisticsModel[]> GetAsync(CategoryStatisticsParameters parameters)
    {
        var query = _appDbContext.CategoryTotalTimeStatistics
            .Where(x =>
                x.AccountId == parameters.AccountId &&
                x.CategoryId == parameters.CategoryId);
        if (parameters.Year.HasValue)
        {
            query = query.Where(x => x.Year == parameters.Year.Value);
        }
        else
        {
            //last 10 years
            var pastYear = DateTime.UtcNow.Year - 10;
            query = query.Where(x => x.Year >= pastYear);
        }
        
        switch (parameters.TimeType)
        {
            case StatisticsTimeType.Week:
                return await query.Where(x => x.WeekNumberOfYear != null)
                    .Select(x => new CategoryStatisticsModel
                    {
                        Value = x.TotalValue,
                        TimeItem = x.WeekNumberOfYear!.Value
                    })
                    .ToArrayAsync();

            case StatisticsTimeType.Month:
                return await query.Where(x => x.Month != null)
                    .Select(x => new CategoryStatisticsModel
                    {
                        Value = x.TotalValue,
                        TimeItem = x.Month!.Value
                    })
                    .ToArrayAsync();

            default:
                return await query
                    .Where(x => x.WeekNumberOfYear == null && x.Month == null)
                    .Select(x => new CategoryStatisticsModel
                    {
                        Value = x.TotalValue,
                        TimeItem = x.Year
                    })
                    .ToArrayAsync();
        }
    }

    public async Task UpdateStatisticsAsync(Guid accountId, Guid categoryId, decimal value, DateTime date)
    {
        try
        {
            var (weekNumberOfYear, month, year) = date.SplitDate();
            var absValue = Math.Abs(value);

            var weekStatistics = await _appDbContext.CategoryTotalTimeStatistics.FirstOrDefaultAsync(
                x => x.AccountId == accountId &&
                     x.CategoryId == categoryId &&
                     x.WeekNumberOfYear == weekNumberOfYear &&
                     x.Year == year);
            if (weekStatistics != null)
            {
                weekStatistics.TotalValue += absValue;
                _appDbContext.CategoryTotalTimeStatistics.Update(weekStatistics);
            }
            else
            {
                await _appDbContext.CategoryTotalTimeStatistics.AddAsync(new CategoryTotalTimeStatistics
                {
                    Id = Guid.NewGuid(),
                    AccountId = accountId,
                    CategoryId = categoryId,
                    TotalValue = absValue,
                    WeekNumberOfYear = weekNumberOfYear,
                    Year = year
                });
            }

            var monthStatistics = await _appDbContext.CategoryTotalTimeStatistics.FirstOrDefaultAsync(
                x => x.AccountId == accountId &&
                     x.CategoryId == categoryId &&
                     x.Month == month &&
                     x.Year == year);
            if (monthStatistics != null)
            {
                monthStatistics.TotalValue += absValue;
                _appDbContext.CategoryTotalTimeStatistics.Update(monthStatistics);
            }
            else
            {
                await _appDbContext.CategoryTotalTimeStatistics.AddAsync(new CategoryTotalTimeStatistics
                {
                    Id = Guid.NewGuid(),
                    AccountId = accountId,
                    CategoryId = categoryId,
                    TotalValue = absValue,
                    Month = month,
                    Year = year
                });
            }

            var yearStatistics = await _appDbContext.CategoryTotalTimeStatistics.FirstOrDefaultAsync(
                x => x.AccountId == accountId &&
                     x.CategoryId == categoryId &&
                     x.WeekNumberOfYear == null &&
                     x.Month == null &&
                     x.Year == year);
            if (yearStatistics != null)
            {
                yearStatistics.TotalValue += absValue;
                _appDbContext.CategoryTotalTimeStatistics.Update(yearStatistics);
            }
            else
            {
                await _appDbContext.CategoryTotalTimeStatistics.AddAsync(new CategoryTotalTimeStatistics
                {
                    Id = Guid.NewGuid(),
                    AccountId = accountId,
                    CategoryId = categoryId,
                    TotalValue = absValue,
                    Year = year
                });
            }

            await _appDbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, 
                "Update category with id {CategoryId} time statistics for account with id {AccountId} error: {ErrorMessage}",
                categoryId,
                accountId,
                e.Message);
        }
    }
}