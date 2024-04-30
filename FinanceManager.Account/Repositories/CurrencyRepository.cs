using FinanceManager.Account.Domain;
using FinanceManager.Account.Models;
using FinanceManager.UnitOfWork.Abstructs;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.Account.Repositories;

public interface ICurrencyRepository : IUnitOfWorkRepository
{
    Task<Currency[]> GetAsync(CurrencySpecification specification);
    Task<bool> CheckExistAsync(CurrencySpecification specification);
    Task<Currency> CreateAsync(string requestId, long userId, string name, string shortName, string? icon);
    Task BulkCreateAsync(Currency[] entities);
    Task DeleteAsync(Currency entity);
    Task BulkDeleteAsync(Currency[] entities);
}

public class CurrencySpecification
{
    public CurrencySpecification() { }

    public CurrencySpecification(
        Guid? id = null,
        long? userId = null,
        string? name = null,
        string? requestId = null)
    {
        Id = id;
        UserId = userId;
        Name = name;
        RequestId = requestId;
    }

    public Guid? Id { get; set; }
    public long? UserId { get; set; }
    public string? Name { get; set; }
    public string? RequestId { get; set; }
}

public class CurrencyRepository : ICurrencyRepository
{
    private readonly AppDbContext _appDbContext;
    private readonly IUnitOfWork _unitOfWork;

    public CurrencyRepository(
        AppDbContext appDbContext,
        IUnitOfWork unitOfWork)
    {
        _appDbContext = appDbContext;
        _unitOfWork = unitOfWork;
    }

    private IQueryable<Currency> BuildQuery(CurrencySpecification specification)
    {
        var query = _appDbContext.Currencies.Where(x => true);

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

        return query;
    }

    public async Task<Currency[]> GetAsync(CurrencySpecification specification)
    {
        var query = BuildQuery(specification);
        return await query.ToArrayAsync();
    }

    public async Task<bool> CheckExistAsync(CurrencySpecification specification)
    {
        var query = BuildQuery(specification);
        return await query.AnyAsync();
    }

    public async Task<Currency> CreateAsync(string requestId, long userId, string name, string shortName, string? icon)
    {
        var created = await _appDbContext.AddAsync(new Currency
        {
            Id = Guid.NewGuid(),
            RequestId = requestId,
            UserId = userId,
            Name = name,
            ShortName = shortName,
            Icon = icon,
        });
        return created.Entity;
    }

    public Task BulkCreateAsync(Currency[] entities)
    {
        _unitOfWork.BulkOperation = true;
        return _appDbContext.BulkInsertAsync(entities);
    }

    public Task DeleteAsync(Currency entity)
    {
        _appDbContext.Currencies.Remove(entity);
        return Task.CompletedTask;
    }

    public Task BulkDeleteAsync(Currency[] entities)
    {
        _appDbContext.Currencies.RemoveRange(entities);
        return Task.CompletedTask;
    }
}