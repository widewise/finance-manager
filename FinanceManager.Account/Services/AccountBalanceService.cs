using FinanceManager.Account.Models;
using FinanceManager.Account.Repositories;
using FinanceManager.Events;
using FinanceManager.Events.Models;
using FinanceManager.TransportLibrary.Services;
using FinanceManager.UnitOfWork.EntityFramework.Abstracts;

namespace FinanceManager.Account.Services;

public interface IAccountBalanceService
{
    Task<bool> UpdateAccountBalanceAsync(UpdateAccountBalanceModel model);
    Task<bool> TransferBetweenAccountsAsync(TransferBetweenAccountsModel model);
}

public class AccountBalanceService : IAccountBalanceService
{
    private readonly ILogger<AccountService> _logger;
    private readonly IAccountRepository _accountRepository;
    private readonly IAccountLimitRepository _accountLimitRepository;
    private readonly IUnitOfWorkExecuter _unitOfWorkExecuter;
    private readonly IMessagePublisher<NotificationSendEvent> _notificationSendPublisher;
    private readonly IMessagePublisher<ChangeStatisticsEvent> _changeStatisticsPublisher;

    public AccountBalanceService(
        ILogger<AccountService> logger,
        IAccountRepository accountRepository,
        IAccountLimitRepository accountLimitRepository,
        IUnitOfWorkExecuter unitOfWorkExecuter,
        IMessagePublisher<NotificationSendEvent> notificationSendPublisher,
        IMessagePublisher<ChangeStatisticsEvent> changeStatisticsPublisher)
    {
        _logger = logger;
        _accountRepository = accountRepository;
        _accountLimitRepository = accountLimitRepository;
        _unitOfWorkExecuter = unitOfWorkExecuter;
        _notificationSendPublisher = notificationSendPublisher;
        _changeStatisticsPublisher = changeStatisticsPublisher;
    }

    public async Task<bool> UpdateAccountBalanceAsync(UpdateAccountBalanceModel model)
    {
        try
        {
            return await _unitOfWorkExecuter.ExecuteAsync(async uof =>
                await InternalUpdateAccountBalanceAsync(uof.Repository<IAccountRepository>(), model));
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "Updating account with id {AccountId} balance error: {ErrorMessage}",
                model.AccountId,
                e.Message);
            return false;
        }
    }

    public async Task<bool> TransferBetweenAccountsAsync(TransferBetweenAccountsModel model)
    {
        try
        {
            return await _unitOfWorkExecuter.ExecuteAsync(async uof =>
            {
                var accountRepository = uof.Repository<IAccountRepository>();
                if (!await InternalUpdateAccountBalanceAsync(accountRepository, new UpdateAccountBalanceModel
                    {
                        TransactionId = model.TransactionId,
                        AccountId = model.FromAccountId,
                        CategoryId = TransferConstants.TransferCategoryId,
                        Date = model.Date,
                        Value = -model.FromValue,
                        UserAddress = model.UserAddress,
                    }))
                {
                    return false;
                }

                if (!await InternalUpdateAccountBalanceAsync(accountRepository, new UpdateAccountBalanceModel
                    {
                        TransactionId = model.TransactionId,
                        AccountId = model.ToAccountId,
                        CategoryId = TransferConstants.TransferCategoryId,
                        Date = model.Date,
                        Value = model.ToValue,
                        UserAddress = model.UserAddress,
                    }))
                {
                    return false;
                }

                return true;
            });

        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "Transfer from account with id {FromAccountId} to account with id {ToAccountId} error: {ErrorMessage}",
                model.FromAccountId,
                model.ToAccountId,
                e.Message);
            return false;
        }
    }

    private async Task<bool> InternalUpdateAccountBalanceAsync(
        IAccountRepository accountRepository,
        UpdateAccountBalanceModel model)
    {
        var account = (await _accountRepository.GetAsync(new AccountSpecification(id: model.AccountId))).FirstOrDefault();
        if (account == null)
        {
            _logger.LogWarning("Account with id {AccountId} is not found", model.AccountId);
            return false;
        }

        var limits = await _accountLimitRepository.GetAsync(new AccountLimitSpecification(accountId: model.AccountId));

        var validateResult = await account.ValidateAndUpdateBalanceAsync(
            limits,
            model.Value,
            model.Date,
            model.CategoryId,
            model.AccountId,
            model.UserAddress,
            model.TransactionId,
            _logger,
            _notificationSendPublisher,
            _changeStatisticsPublisher);
        if (!validateResult)
        {
            return false;
        }

        await accountRepository.UpdateAsync(account);

        return true;
    }
}