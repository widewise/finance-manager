using FinanceManager.UnitOfWork.Abstructs;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.Account.Repositories;

public interface IAccountRepository : IUnitOfWorkRepository
{
    Task<Domain.Account[]> GetAsync(AccountSpecification specification);
    Task<bool> CheckExistAsync(AccountSpecification specification);
    Task<Domain.Account> CreateAsync(
        string requestId,
        long userId,
        Guid currencyId,
        string name,
        string? description,
        string? icon);
    Task BulkCreateAsync(Domain.Account[] entities);
    Task UpdateAsync(Domain.Account entity);
    Task DeleteAsync(Domain.Account entity);
    Task BulkDeleteAsync(Domain.Account[] entities);
}

public class AccountSpecification
{
    public AccountSpecification() { }
    public AccountSpecification(
        Guid? id = null,
        long? userId = null,
        string? requestId = null,
        string? name = null,
        Guid? currencyId = null)
    {
        Id = id;
        UserId = userId;
        RequestId = requestId;
        Name = name;
        CurrencyId = currencyId;
    }

    public Guid? Id { get; set; }
    public long? UserId { get; set; }
    public string? RequestId { get; set; }
    public string? Name { get; set; }
    public Guid? CurrencyId { get; set; }
}

public class AccountRepository: IAccountRepository
{
    private readonly AppDbContext _appDbContext;
    private readonly IUnitOfWork _unitOfWork;

    public AccountRepository(
        AppDbContext appDbContext,
        IUnitOfWork unitOfWork)
    {
        _appDbContext = appDbContext;
        _unitOfWork = unitOfWork;
    }

    private IQueryable<Domain.Account> BuildQuery(AccountSpecification specification)
    {
        var query = _appDbContext.Accounts.Where(x => true);
        if (specification.Id.HasValue)
        {
            query = query.Where(x => x.Id == specification.Id.Value);
        }

        if (specification.UserId.HasValue)
        {
            query = query.Where(x => x.UserId == specification.UserId.Value);
        }

        if (specification.RequestId != null)
        {
            query = query.Where(x => x.RequestId == specification.RequestId);
        }

        if (specification.Name != null)
        {
            query = query.Where(x => x.Name == specification.Name);
        }

        if (specification.CurrencyId != null)
        {
            query = query.Where(x => x.CurrencyId == specification.CurrencyId);
        }

        return query;
    }

    public async Task<Domain.Account[]> GetAsync(AccountSpecification specification)
    {
        var query = BuildQuery(specification);
        return await query.ToArrayAsync();
    }

    public async Task<bool> CheckExistAsync(AccountSpecification specification)
    {
        var query = BuildQuery(specification);
        return await query.AnyAsync();
    }

    public async Task<Domain.Account> CreateAsync(
        string requestId,
        long userId,
        Guid currencyId,
        string name,
        string? description,
        string? icon)
    {
        var created = await _appDbContext.AddAsync(new Domain.Account
        {
            Id = Guid.NewGuid(),
            RequestId = requestId,
            UserId = userId,
            CurrencyId = currencyId,
            Name = name,
            Description = description,
            Balance = 0,
            Icon = icon
        });

        return created.Entity;
    }

    public Task BulkCreateAsync(Domain.Account[] entities)
    {
        _unitOfWork.BulkOperation = true;
        return _appDbContext.BulkInsertAsync(entities);
    }

    public Task UpdateAsync(Domain.Account entity)
    {
        _appDbContext.Accounts.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Domain.Account entity)
    {
        _appDbContext.Accounts.Remove(entity);
        return Task.CompletedTask;
    }

    public Task BulkDeleteAsync(Domain.Account[] entities)
    {
        _appDbContext.Accounts.RemoveRange(entities);
        return Task.CompletedTask;
    }
}