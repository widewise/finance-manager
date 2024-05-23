using AutoMapper;
using FinanceManager.Account.Models;
using FinanceManager.Account.Repositories;
using FinanceManager.UnitOfWork.EntityFramework.Abstracts;

namespace FinanceManager.Account.Services;

public interface IAccountService
{
    Task<Domain.Account[]> GetAsync(AccountQueryParameters parameters);
    Task<Domain.Account?> CreateAsync(CreateAccountModel model, long userId, string requestId);
    Task<Domain.Account[]?> BulkCreateAsync(
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
    private readonly IMapper _mapper;
    private readonly IAccountRepository _accountRepository;
    private readonly IAccountLimitRepository _accountLimitRepository;
    private readonly IUnitOfWorkExecuter _unitOfWorkExecuter;

    public AccountService(
        ILogger<AccountService> logger,
        IMapper mapper,
        IAccountRepository accountRepository,
        IAccountLimitRepository accountLimitRepository,
        IUnitOfWorkExecuter unitOfWorkExecuter)
    {
        _logger = logger;
        _mapper = mapper;
        _accountRepository = accountRepository;
        _accountLimitRepository = accountLimitRepository;
        _unitOfWorkExecuter = unitOfWorkExecuter;
    }

    public async Task<Domain.Account[]> GetAsync(AccountQueryParameters parameters)
    {
        var specification = _mapper.Map<AccountSpecification>(parameters);
        return await _accountRepository.GetAsync(specification);
    }

    public async Task<Domain.Account?> CreateAsync(
        CreateAccountModel model,
        long userId,
        string requestId)
    {
        if (await _accountRepository.CheckExistAsync(new AccountSpecification(requestId: requestId)))
        {
            _logger.LogWarning(
                "Account has already created for request with id {RequestId}",
                requestId);
            return null;
        }

        if (await _accountRepository.CheckExistAsync(new AccountSpecification(userId: userId, name: model.Name)))
        {
            _logger.LogWarning(
                "Account with name {Name} has already created",
                model.Name);
            return null;
        }

        var created = await _unitOfWorkExecuter.ExecuteAsync<IAccountRepository, Domain.Account>(
            repo => repo.CreateAsync(requestId, userId, model.CurrencyId, model.Name, model.Description, model.Icon));

        return created;
    }

    public async Task<Domain.Account[]?> BulkCreateAsync(
        CreateAccountModel[] models,
        long userId,
        string requestId)
    {
        if (await _accountRepository.CheckExistAsync(new AccountSpecification(requestId: requestId)))
        {
            _logger.LogWarning(
                "Accounts has already created for request with id {RequestId}",
                requestId);
            return null;
        }

        var newEntities = models.Select(model => new Domain.Account
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

        await _unitOfWorkExecuter.ExecuteAsync<IAccountRepository>(
            repo => repo.BulkCreateAsync(newEntities));

        return newEntities;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateAccountModel model)
    {
        var existed = (await _accountRepository.GetAsync(new AccountSpecification(id: id))).FirstOrDefault();
        if (existed == null)
        {
            _logger.LogWarning("Account with id {d} is not found", id);
            return false;
        }

        existed.Name = model.Name;
        existed.Description = model.Description;
        existed.Icon = model.Icon;
        
        await _unitOfWorkExecuter.ExecuteAsync<IAccountRepository>(
            repo => repo.UpdateAsync(existed));

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existed = (await _accountRepository.GetAsync(new AccountSpecification(id: id))).FirstOrDefault();
        if (existed == null)
        {
            _logger.LogWarning("Account with id {d} is not found", id);
            return false;
        }

        var limits = await _accountLimitRepository.GetAsync(new AccountLimitSpecification(accountId: id));
        await _unitOfWorkExecuter.ExecuteAsync(async uof =>
        {
            await uof.Repository<IAccountLimitRepository>().BulkDeleteAsync(limits);
            await uof.Repository<IAccountRepository>().DeleteAsync(existed);
            return 0;
        });
        
        return true;
    }

    public async Task RejectAsync(string requestId)
    {
        var accounts = await _accountRepository.GetAsync(new AccountSpecification(requestId:requestId));
        if (!accounts.Any())
        {
            return;
        }

        var accountIds = accounts.Select(x => x.Id).ToArray();
        var limits = await _accountLimitRepository.GetAsync(new AccountLimitSpecification(accountIds: accountIds));

        await _unitOfWorkExecuter.ExecuteAsync(async uof =>
        {
            if (limits.Any())
            {
                await uof.Repository<IAccountLimitRepository>().BulkDeleteAsync(limits);
            }
            await uof.Repository<IAccountRepository>().BulkDeleteAsync(accounts);
            return 0;
        });
    }
}