using FinanceManager.Account.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.Account.Services;

public interface IAccountLimitService
{
    Task<AccountLimit[]> GetAsync(Guid accountId);
    Task<AccountLimit?> CreateAsync(CreateAccountLimitModel model, string requestId);
    Task<bool> UpdateAsync(Guid id, UpdateAccountLimitModel model);
    Task<bool> DeleteAsync(Guid id);
}

public class AccountLimitService : IAccountLimitService
{
    private readonly ILogger<AccountLimitService> _logger;
    private readonly AppDbContext _appDbContext;

    public AccountLimitService(
        ILogger<AccountLimitService> logger,
        AppDbContext appDbContext)
    {
        _logger = logger;
        _appDbContext = appDbContext;
    }

    public async Task<AccountLimit[]> GetAsync(Guid accountId)
    {
        return await _appDbContext.AccountLimits
            .Where(x => x.AccountId == accountId)
            .ToArrayAsync();
    }

    public async Task<AccountLimit?> CreateAsync(
        CreateAccountLimitModel model,
        string requestId)
    {
        if (await _appDbContext.AccountLimits.AnyAsync(x => x.RequestId == requestId))
        {
            _logger.LogWarning(
                "AccountLimit has already created for request with id {RequestId}",
                requestId);
            return null;
        }

        var limit = new AccountLimit
        {
            Id = Guid.NewGuid(),
            RequestId = requestId,
            AccountId = model.AccountId,
            Type = model.Type,
            Time = model.Time,
            Description = model.Description,
            LimitValue = model.LimitValue
        };
        var created = await _appDbContext.AddAsync(limit);

        await _appDbContext.SaveChangesAsync();

        return created.Entity;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateAccountLimitModel model)
    {
        var existed = await _appDbContext.AccountLimits.FirstOrDefaultAsync(x => x.Id == id);
        if (existed == null)
        {
            _logger.LogWarning("AccountLimit with id {d} is not found", id);
            return false;
        }

        existed.Description = model.Description;
        _appDbContext.AccountLimits.Update(existed);
        await _appDbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existed = await _appDbContext.AccountLimits.FirstOrDefaultAsync(x => x.Id == id);
        if (existed == null)
        {
            _logger.LogWarning("AccountLimit with id {d} is not found", id);
            return false;
        }

        _appDbContext.AccountLimits.Remove(existed);
        await _appDbContext.SaveChangesAsync();
        return true;
    }
}