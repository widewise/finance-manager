using FinanceManager.Account.Domain;
using FinanceManager.Account.Models;
using FinanceManager.Account.Repositories;
using FinanceManager.UnitOfWork.EntityFramework.Abstracts;

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
    private readonly IAccountLimitRepository _accountLimitRepository;
    private readonly IUnitOfWorkExecuter _unitOfWorkExecuter;

    public AccountLimitService(
        ILogger<AccountLimitService> logger,
        IAccountLimitRepository accountLimitRepository,
        IUnitOfWorkExecuter unitOfWorkExecuter)
    {
        _logger = logger;
        _accountLimitRepository = accountLimitRepository;
        _unitOfWorkExecuter = unitOfWorkExecuter;
    }

    public Task<AccountLimit[]> GetAsync(Guid accountId)
    {
        return _accountLimitRepository.GetAsync(new AccountLimitSpecification(accountId: accountId));
    }

    public async Task<AccountLimit?> CreateAsync(
        CreateAccountLimitModel model,
        string requestId)
    {
        if (await _accountLimitRepository.CheckExistAsync(new AccountLimitSpecification(requestId: requestId)))
        {
            _logger.LogWarning(
                "AccountLimit has already created for request with id {RequestId}",
                requestId);
            return null;
        }

        return await _unitOfWorkExecuter.ExecuteAsync<IAccountLimitRepository, AccountLimit>(
            repo => repo.CreateAsync(
                requestId,
                model.AccountId,
                model.Type,
                model.Time,
                model.Description,
                model.LimitValue));
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateAccountLimitModel model)
    {
        var existed = (await _accountLimitRepository.GetAsync(new AccountLimitSpecification(id: id))).FirstOrDefault();
        if (existed == null)
        {
            _logger.LogWarning("AccountLimit with id {Id} is not found", id);
            return false;
        }

        existed.Description = model.Description;
        await _unitOfWorkExecuter.ExecuteAsync<IAccountLimitRepository>(
            repo => repo.UpdateAsync(existed));
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existed = (await _accountLimitRepository.GetAsync(new AccountLimitSpecification(id: id))).FirstOrDefault();
        if (existed == null)
        {
            _logger.LogWarning("AccountLimit with id {Id} is not found", id);
            return false;
        }

        await _unitOfWorkExecuter.ExecuteAsync<IAccountLimitRepository>(
            repo => repo.DeleteAsync(existed));
        return true;
    }
}