using FinanceManager.Account.Domain;
using FinanceManager.Account.Models;
using FinanceManager.UnitOfWork.Abstructs;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.Account.Repositories;

public interface IAccountLimitRepository : IUnitOfWorkRepository
{
    Task<AccountLimit[]> GetAsync(AccountLimitSpecification specification);
    Task<bool> CheckExistAsync(AccountLimitSpecification specification);
    Task<AccountLimit> CreateAsync(
        string requestId,
        Guid accountId,
        AccountLimitType type,
        AccountLimitTime time,
        string? description,
        decimal limitValue);
    Task UpdateAsync(AccountLimit entity);
    Task DeleteAsync(AccountLimit entity);
    Task BulkDeleteAsync(AccountLimit[] entities);
}

public class AccountLimitSpecification
{
    public AccountLimitSpecification(
        Guid? id = null,
        Guid? accountId = null,
        string? requestId = null,
        Guid[]? accountIds = null)
    {
        Id = id;
        AccountId = accountId;
        AccountIds = accountIds;
        RequestId = requestId;
    }

    public Guid? Id { get; set; }
    public Guid? AccountId { get; set; }
    public Guid[]? AccountIds { get; set; }
    public string? RequestId { get; set; }
}

public class AccountLimitRepository : IAccountLimitRepository
{
    private readonly AppDbContext _appDbContext;

    public AccountLimitRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    private IQueryable<AccountLimit> BuildQuery(AccountLimitSpecification specification)
    {
        var query = _appDbContext.AccountLimits.Where(x => true);

        if (specification.Id != null)
        {
            query = query.Where(x => x.Id == specification.Id);
        }

        if (specification.RequestId != null)
        {
            query = query.Where(x => x.RequestId == specification.RequestId);
        }

        if (specification.AccountId != null)
        {
            query = query.Where(x => x.AccountId == specification.AccountId);
        }

        if (specification.AccountIds != null)
        {
            query = query.Where(x => specification.AccountIds.Contains(x.AccountId));
        }

        return query;
    }

    public async Task<AccountLimit[]> GetAsync(AccountLimitSpecification specification)
    {
        var query = BuildQuery(specification);
        return await query.ToArrayAsync();
    }

    public async Task<bool> CheckExistAsync(AccountLimitSpecification specification)
    {
        var query = BuildQuery(specification);
        return await query.AnyAsync();
    }

    public async Task<AccountLimit> CreateAsync(string requestId, Guid accountId, AccountLimitType type, AccountLimitTime time, string? description, decimal limitValue)
    {
        var created = await _appDbContext.AddAsync(new AccountLimit
        {
            Id = Guid.NewGuid(),
            RequestId = requestId,
            AccountId = accountId,
            Type = type,
            Time = time,
            Description = description,
            LimitValue = limitValue
        });
        return created.Entity;
    }

    public Task UpdateAsync(AccountLimit entity)
    {
        _appDbContext.AccountLimits.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(AccountLimit entity)
    {
        _appDbContext.AccountLimits.Remove(entity);
        return Task.CompletedTask;
    }

    public Task BulkDeleteAsync(AccountLimit[] entities)
    {
        _appDbContext.AccountLimits.RemoveRange(entities);
        return Task.CompletedTask;
    }
}