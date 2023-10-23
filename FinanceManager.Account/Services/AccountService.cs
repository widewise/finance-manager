using FinanceManager.Account.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.Account.Services;

public interface IAccountService
{
    Task<Models.Account[]> GetAsync(AccountQueryParameters parameters);
    Task<Models.Account?> CreateAsync(CreateAccountModel model, long userId, string requestId);
    Task<Models.Account[]?> BulkCreateAsync(
        CreateAccountModel[] models,
        long userId,
        string requestId);
    Task<bool> UpdateAsync(Guid id, UpdateAccountModel model);
    Task<bool> DeleteAsync(Guid id);
    Task RejectAsync(string requestId);
}

public class AccountService : IAccountService
{
    private readonly ILogger<AccountService> _logger;
    private readonly AppDbContext _appDbContext;

    public AccountService(
        ILogger<AccountService> logger,
        AppDbContext appDbContext)
    {
        _logger = logger;
        _appDbContext = appDbContext;
    }

    public async Task<Models.Account[]> GetAsync(AccountQueryParameters parameters)
    {
        var query = _appDbContext.Accounts.Where(x => true);
        if (parameters.Id.HasValue)
        {
            query = query.Where(x => x.Id == parameters.Id.Value);
        }

        if (parameters.UserId.HasValue)
        {
            query = query.Where(x => x.UserId == parameters.UserId.Value);
        }

        if (parameters.RequestId != null)
        {
            query = query.Where(x => x.RequestId == parameters.RequestId);
        }
            
        return await query.ToArrayAsync();
    }

    public async Task<Models.Account?> CreateAsync(
        CreateAccountModel model,
        long userId,
        string requestId)
    {
        if (await _appDbContext.Accounts.AnyAsync(x => x.RequestId == requestId))
        {
            _logger.LogWarning(
                "Account has already created for request with id {RequestId}",
                requestId);
            return null;
        }

        if (await _appDbContext.Accounts.AnyAsync(
                x => x.Name == model.Name && x.UserId == userId))
        {
            _logger.LogWarning(
                "Account with name {Name} has already created",
                model.Name);
            return null;
        }

        var created = await _appDbContext.AddAsync(new Models.Account
        {
            Id = Guid.NewGuid(),
            RequestId = requestId,
            UserId = userId,
            CurrencyId = model.CurrencyId,
            Name = model.Name,
            Description = model.Description,
            Balance = 0,
            Icon = model.Icon
        });

        await _appDbContext.SaveChangesAsync();

        return created.Entity;
    }

    public async Task<Models.Account[]?> BulkCreateAsync(
        CreateAccountModel[] models,
        long userId,
        string requestId)
    {
        if (await _appDbContext.Accounts.AnyAsync(x => x.RequestId == requestId))
        {
            _logger.LogWarning(
                "Accounts has already created for request with id {RequestId}",
                requestId);
            return null;
        }

        var newEntities = models.Select(model => new Models.Account
        {
            Id = Guid.NewGuid(),
            RequestId = requestId,
            UserId = userId,
            CurrencyId = model.CurrencyId,
            Name = model.Name,
            Description = model.Description,
            Balance = 0,
            Icon = model.Icon
        }).ToArray();

        await _appDbContext.BulkInsertAsync(newEntities);
        await _appDbContext.BulkSaveChangesAsync();

        return newEntities;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateAccountModel model)
    {
        var existed = await _appDbContext.Accounts.FirstOrDefaultAsync(x => x.Id == id);
        if (existed == null)
        {
            _logger.LogWarning("Account with id {d} is not found", id);
            return false;
        }

        existed.Name = model.Name;
        existed.Description = model.Description;
        existed.Icon = model.Icon;
        _appDbContext.Accounts.Update(existed);
        await _appDbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existed = await _appDbContext.Accounts.FirstOrDefaultAsync(x => x.Id == id);
        if (existed == null)
        {
            _logger.LogWarning("Account with id {d} is not found", id);
            return false;
        }

        var limits = _appDbContext.AccountLimits.Where(x => x.AccountId == id);
        _appDbContext.AccountLimits.RemoveRange(limits);
        _appDbContext.Accounts.Remove(existed);
        await _appDbContext.SaveChangesAsync();
        return true;
    }

    public async Task RejectAsync(string requestId)
    {
        var accounts = await _appDbContext.Accounts
            .Where(x => x.RequestId == requestId)
            .ToArrayAsync();
        if (!accounts.Any())
        {
            return;
        }

        var accountIds = accounts.Select(x => x.Id).ToArray();
        var limits = await _appDbContext.AccountLimits
            .Where(x => accountIds.Contains(x.AccountId))
            .ToArrayAsync();

        if (limits.Any())
        {
            _appDbContext.AccountLimits.RemoveRange(limits);
        }

        _appDbContext.Accounts.RemoveRange(accounts);
        await _appDbContext.SaveChangesAsync();
    }
}