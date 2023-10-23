using FinanceManager.Account.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.Account.Services;

public interface ICurrencyService
{
    Task<Currency[]> GetAsync(CurrencyQueryParameters parameters);
    Task<Currency?> CreateAsync(string requestId, long userId, CreateCurrencyModel model);
    Task<Currency[]?> BulkCreateAsync(
        CreateCurrencyModel[] models,
        long userId,
        string requestId);
    Task<bool> DeleteAsync(Guid id, long userId);
    Task RejectAsync(string requestId);
}

public class CurrencyService: ICurrencyService
{
    private readonly ILogger<CurrencyService> _logger;
    private readonly AppDbContext _appDbContext;

    public CurrencyService(
        ILogger<CurrencyService> logger,
        AppDbContext appDbContext)
    {
        _logger = logger;
        _appDbContext = appDbContext;
    }

    public async Task<Currency[]> GetAsync(CurrencyQueryParameters parameters)
    {
        var query = _appDbContext.Currencies.Where(x => true);

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

    public async Task<Currency?> CreateAsync(
        string requestId,
        long userId,
        CreateCurrencyModel model)
    {
        if (await _appDbContext.Currencies.AnyAsync(x => x.RequestId == requestId))
        {
            _logger.LogWarning(
                "Currency has already created for request with id {RequestId}",
                requestId);
            return null;
        }

        if (await _appDbContext.Currencies.AnyAsync(
                x => x.Name == model.Name && x.UserId == userId))
        {
            _logger.LogWarning(
                "Currency with name {Name} has already created",
                model.Name);
            return null;
        }

        var created = await _appDbContext.AddAsync(new Currency
        {
            Id = Guid.NewGuid(),
            RequestId = requestId,
            UserId = userId,
            Name = model.Name,
            ShortName = model.ShortName,
            Icon = model.Icon,
        });
        
        await _appDbContext.SaveChangesAsync();

        return created.Entity;
    }

    public async Task<Currency[]?> BulkCreateAsync(
        CreateCurrencyModel[] models,
        long userId,
        string requestId)
    {
        if (await _appDbContext.Currencies.AnyAsync(x => x.RequestId == requestId))
        {
            _logger.LogWarning(
                "Currencies has already created for request with id {RequestId}",
                requestId);
            return null;
        }

        var newEntities = models.Select(model => new Currency
        {
            Id = Guid.NewGuid(),
            RequestId = requestId,
            UserId = userId,
            Name = model.Name,
            ShortName = model.ShortName,
            Icon = model.Icon,
        }).ToArray();

        await _appDbContext.BulkInsertAsync(newEntities);
        await _appDbContext.BulkSaveChangesAsync();

        return newEntities;
    }

    public async Task<bool> DeleteAsync(Guid id, long userId)
    {
        var existed = await _appDbContext.Currencies.FirstOrDefaultAsync(x => x.Id == id);
        if (existed == null)
        {
            _logger.LogWarning("Category with id {d} is not found", id);
            return false;
        }

        if (existed.UserId != userId)
        {
            _logger.LogWarning("The currency was created by another user");
            return false;
        }

        if (await _appDbContext.Accounts.AnyAsync(x => x.CurrencyId == id))
        {
            _logger.LogWarning("There are accounts with currency with id {Id}", id);
            return false;
        }

        _appDbContext.Currencies.Remove(existed);
        await _appDbContext.SaveChangesAsync();
        return true;
    }

    public async Task RejectAsync(string requestId)
    {
        var currencies = await _appDbContext.Currencies
            .Where(x => x.RequestId == requestId)
            .ToArrayAsync();
        if (!currencies.Any())
        {
            return;
        }

        _appDbContext.Currencies.RemoveRange(currencies);
        await _appDbContext.SaveChangesAsync();
    }
}