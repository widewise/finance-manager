using AutoMapper;
using FinanceManager.Account.Domain;
using FinanceManager.Account.Models;
using FinanceManager.Account.Repositories;
using FinanceManager.UnitOfWork.EntityFramework.Abstracts;

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
    private readonly IMapper _mapper;
    private readonly ICurrencyRepository _currencyRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWorkExecuter _unitOfWorkExecuter;

    public CurrencyService(
        ILogger<CurrencyService> logger,
        IMapper mapper,
        ICurrencyRepository currencyRepository,
        IAccountRepository accountRepository,
        IUnitOfWorkExecuter unitOfWorkExecuter)
    {
        _logger = logger;
        _mapper = mapper;
        _currencyRepository = currencyRepository;
        _accountRepository = accountRepository;
        _unitOfWorkExecuter = unitOfWorkExecuter;
    }

    public async Task<Currency[]> GetAsync(CurrencyQueryParameters parameters)
    {
        var specification = _mapper.Map<CurrencySpecification>(parameters);
        return await _currencyRepository.GetAsync(specification);
    }

    public async Task<Currency?> CreateAsync(
        string requestId,
        long userId,
        CreateCurrencyModel model)
    {
        if (await _currencyRepository.CheckExistAsync(new CurrencySpecification(requestId: requestId)))
        {
            _logger.LogWarning(
                "Currency has already created for request with id {RequestId}",
                requestId);
            return null;
        }

        if (await _currencyRepository.CheckExistAsync(new CurrencySpecification(userId: userId, name: model.Name)))
        {
            _logger.LogWarning(
                "Currency with name {Name} has already created",
                model.Name);
            return null;
        }

        var created = await _unitOfWorkExecuter.ExecuteAsync<ICurrencyRepository, Currency>(
            repo => repo.CreateAsync(requestId, userId, model.Name, model.ShortName, model.Icon));

        return created;
    }

    public async Task<Currency[]?> BulkCreateAsync(
        CreateCurrencyModel[] models,
        long userId,
        string requestId)
    {
        if (await _currencyRepository.CheckExistAsync(new CurrencySpecification(requestId: requestId)))
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

        await _unitOfWorkExecuter.ExecuteAsync<ICurrencyRepository>(
            repo => repo.BulkCreateAsync(newEntities));

        return newEntities;
    }

    public async Task<bool> DeleteAsync(Guid id, long userId)
    {
        var existed = (await _currencyRepository.GetAsync(new CurrencySpecification(id: id))).FirstOrDefault();
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

        if (await _accountRepository.CheckExistAsync(new AccountSpecification(currencyId: id)))
        {
            _logger.LogWarning("There are accounts with currency with id {Id}", id);
            return false;
        }

        await _unitOfWorkExecuter.ExecuteAsync<ICurrencyRepository>(
            repo => repo.DeleteAsync(existed));

        return true;
    }

    public async Task RejectAsync(string requestId)
    {
        var currencies = await _currencyRepository.GetAsync(new CurrencySpecification(requestId: requestId));
        if (!currencies.Any())
        {
            return;
        }

        await _unitOfWorkExecuter.ExecuteAsync<ICurrencyRepository>(
            repo => repo.BulkDeleteAsync(currencies));
    }
}