using FinanceManager.Events.Models;
using FinanceManager.File.Exceptions;
using FinanceManager.File.Models;
using FinanceManager.TransportLibrary.Services;
using Microsoft.Extensions.Options;

namespace FinanceManager.File.Services;

public interface IImportMoneyTransactionsService
{
    Task ImportAsync(
        ImportSession session,
        Dictionary<string, Guid> categories,
        Dictionary<string, Guid> accounts,
        FileContentItem[] fileItems);
}

public class ImportMoneyTransactionsService : IImportMoneyTransactionsService
{
    private readonly ILogger<ImportMoneyTransactionsService> _logger;
    private readonly IFinanceManagerRestClient _restClient;
    private readonly ImportDataSettings _settings;
    private readonly IMessagePublisher<AddDepositEvent> _addDepositPublisher;
    private readonly IMessagePublisher<AddExpenseEvent> _addExpensePublisher;
    private readonly IMessagePublisher<AddTransferEvent> _addTransferPublisher;

    public ImportMoneyTransactionsService(
        ILogger<ImportMoneyTransactionsService> logger,
        IFinanceManagerRestClient restClient,
        IOptions<ImportDataSettings> settings,
        IMessagePublisher<AddDepositEvent> addDepositPublisher,
        IMessagePublisher<AddExpenseEvent> addExpensePublisher,
        IMessagePublisher<AddTransferEvent> addTransferPublisher)
    {
        _logger = logger;
        _restClient = restClient;
        _settings = settings.Value;
        _addDepositPublisher = addDepositPublisher;
        _addExpensePublisher = addExpensePublisher;
        _addTransferPublisher = addTransferPublisher;
    }

    public async Task ImportAsync(
        ImportSession session,
        Dictionary<string, Guid> categories,
        Dictionary<string, Guid> accounts,
        FileContentItem[] fileItems)
    {
        //TODO: parallel
        await ImportDepositsAsync(session, categories, accounts, fileItems);
        await ImportTransfersAsync(session, accounts, fileItems);
        await ImportExpensesAsync(session, categories, accounts, fileItems);
    }

    private async Task ImportDepositsAsync(
        ImportSession session,
        Dictionary<string, Guid> categories,
        Dictionary<string, Guid> accounts,
        FileContentItem[] fileItems)
    {
        var deposits = fileItems
            .Where(x => !string.IsNullOrEmpty(x.ToAccountName) &&
                        !string.IsNullOrEmpty(x.ToCategoryName) &&
                        x.ToValue > 0 &&
                        string.IsNullOrEmpty(x.FromAccountName) &&
                        string.IsNullOrEmpty(x.FromCategoryName) &&
                        !x.FromValue.HasValue)
            .Select(x =>
            {
                Guid? accountId = null;
                if(accounts.TryGetValue(x.ToAccountName!, out Guid val1))
                {
                    accountId = val1;
                }

                Guid? categoryId = null;
                if (categories.TryGetValue(x.ToCategoryName!, out Guid val2))
                {
                    categoryId = val2;
                }
                return new
                {
                    AccountId = accountId,
                    CategoryId = categoryId,
                    x.ToValue!.Value,
                    x.Date
                };
            })
            .ToArray();
        var externalDeposits = await _restClient.GetDepositsByTransactionIdAsync(null, session.RequestId);
        if (externalDeposits.Length == 0)
        {
            foreach (var deposit in deposits)
            {
                if (!deposit.AccountId.HasValue || !deposit.CategoryId.HasValue)
                {
                    _logger.LogError(
                        "Deposit with incorrect account id or category id was found for session with id {SessionId}",
                        session.Id);
                    throw new ImportDataException();
                }

                await _addDepositPublisher.SendAsync(new AddDepositEvent
                {
                    TransactionId = session.RequestId,
                    UserId = session.UserId,
                    AccountId = deposit.AccountId.Value,
                    CategoryId = deposit.CategoryId.Value,
                    Date = deposit.Date,
                    Value = deposit.Value
                });
            }

            var importedCount = 0;
            var attempts = 0;
            while (importedCount < deposits.Length || attempts < _settings.WaitingUpdatesAttempts)
            {
                await Task.Delay(TimeSpan.FromSeconds(_settings.WaitingUpdatesTimeoutInSeconds));

                var importedItems = await _restClient.GetDepositsByTransactionIdAsync(null, session.RequestId);
                importedCount = importedItems.Length;
                attempts++;
            }

            if (importedCount < deposits.Length && attempts >= _settings.WaitingUpdatesAttempts)
            {
                _logger.LogError(
                    "Number of attempts exceeded for session with id {SessionId} while waiting for data import",
                    session.Id);
                throw new ImportDataException();
            }
        }
        else if (externalDeposits.Length < deposits.Length)
        {
            _logger.LogError(
                "Session with id {SessionId} was broken incorrect way",
                session.Id);
            throw new ImportDataException();
        }
    }

    private async Task ImportExpensesAsync(
        ImportSession session,
        Dictionary<string, Guid> categories,
        Dictionary<string, Guid> accounts,
        FileContentItem[] fileItems)
    {
        var expenses = fileItems
            .Where(x => !string.IsNullOrEmpty(x.FromAccountName) &&
                        !string.IsNullOrEmpty(x.FromCategoryName) &&
                        x.FromValue > 0 &&
                        string.IsNullOrEmpty(x.ToAccountName) &&
                        string.IsNullOrEmpty(x.ToCategoryName) &&
                        !x.ToValue.HasValue)
            .Select(x =>
            {
                Guid? accountId = null;
                if(accounts.TryGetValue(x.FromAccountName!, out Guid val1))
                {
                    accountId = val1;
                }

                Guid? categoryId = null;
                if (categories.TryGetValue(x.FromCategoryName!, out Guid val2))
                {
                    categoryId = val2;
                }

                return new
                {
                    AccountId = accountId,
                    CategoryId = categoryId,
                    x.FromValue!.Value,
                    x.Date
                };
            })
            .ToArray();
        foreach (var expense in expenses)
        {
            if (!expense.AccountId.HasValue || !expense.CategoryId.HasValue)
            {
                _logger.LogError(
                    "Expense with incorrect account id or category id was found for session with id {SessionId}",
                    session.Id);
                throw new ImportDataException();
            }
            await _addExpensePublisher.SendAsync(new AddExpenseEvent
            {
                TransactionId = session.RequestId,
                UserId = session.UserId,
                AccountId = expense.AccountId.Value,
                CategoryId = expense.CategoryId.Value,
                Date = expense.Date,
                Value = expense.Value
            });
        }
    }

    private async Task ImportTransfersAsync(
        ImportSession session,
        Dictionary<string, Guid> accounts,
        FileContentItem[] fileItems)
    {
        var transfers = fileItems
            .Where(x => !string.IsNullOrEmpty(x.FromAccountName) &&
                        x.FromValue > 0 &&
                        !string.IsNullOrEmpty(x.ToAccountName) &&
                        x.ToValue > 0)
            .Select(x =>
            {
                Guid? fromAccountId = null;
                if(accounts.TryGetValue(x.FromAccountName!, out Guid val1))
                {
                    fromAccountId = val1;
                }

                Guid? toAccountName = null;
                if (accounts.TryGetValue(x.ToAccountName!, out Guid val2))
                {
                    toAccountName = val2;
                }

                return new
                {
                    FromAccountId = fromAccountId,
                    FromValue = x.FromValue!.Value,
                    ToAccountId = toAccountName,
                    ToValue = x.ToValue!.Value,
                    x.Date,
                    x.Description
                };
            })
            .ToArray();
        var externalTransfers = await _restClient.GetTransfersByTransactionIdAsync(null, session.RequestId);
        if (externalTransfers.Length == 0)
        {
            foreach (var transfer in transfers)
            {
                if (!transfer.FromAccountId.HasValue || !transfer.ToAccountId.HasValue)
                {
                    _logger.LogError(
                        "Transfer with incorrect from/to account id was found for session with id {SessionId}",
                        session.Id);
                    throw new ImportDataException();
                }
                await _addTransferPublisher.SendAsync(new AddTransferEvent
                {
                    Id = Guid.NewGuid(),
                    TransactionId = session.RequestId,
                    FromAccountId = transfer.FromAccountId.Value,
                    ToAccountId = transfer.ToAccountId.Value,
                    UserId = session.UserId,
                    Date = transfer.Date,
                    FromValue = transfer.FromValue,
                    ToValue = transfer.ToValue,
                    Description = transfer.Description
                });
            }

            var importedCount = 0;
            var attempts = 0;
            while (importedCount < transfers.Length || attempts < _settings.WaitingUpdatesAttempts)
            {
                await Task.Delay(TimeSpan.FromSeconds(_settings.WaitingUpdatesTimeoutInSeconds));

                var importedItems = await _restClient.GetTransfersByTransactionIdAsync(null, session.RequestId);
                importedCount = importedItems.Length;
                attempts++;
            }

            if (importedCount < transfers.Length && attempts >= _settings.WaitingUpdatesAttempts)
            {
                _logger.LogError(
                    "Number of attempts exceeded for session with id {SessionId} while waiting for data import",
                    session.Id);
                throw new ImportDataException();
            }
        }
        else if (externalTransfers.Length < transfers.Length)
        {
            _logger.LogError(
                "Session with id {SessionId} was broken incorrect way",
                session.Id);
            throw new ImportDataException();
        }
    }
}