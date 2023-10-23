using FinanceManager.File.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.File.Services;

public interface IExportDataService
{
    Task ExecuteAsync();
}

public class ExportDataService : IExportDataService
{
    private readonly ILogger<ExportDataService> _logger;
    private readonly IFinanceManagerRestClient _financeManagerRestClient;
    private readonly FileAppDbContext _fileAppDbContext;
    private readonly ISessionFileSerializer _sessionFileSerializer;

    public ExportDataService(
        ILogger<ExportDataService> logger,
        IFinanceManagerRestClient financeManagerRestClient,
        FileAppDbContext fileAppDbContext,
        ISessionFileSerializer sessionFileSerializer)
    {
        _logger = logger;
        _financeManagerRestClient = financeManagerRestClient;
        _fileAppDbContext = fileAppDbContext;
        _sessionFileSerializer = sessionFileSerializer;
    }

    public async Task ExecuteAsync()
    {
        var session = await _fileAppDbContext.ExportSessions
            .OrderBy(x => x.DateTime)
            .FirstOrDefaultAsync(
                x => x.State == SessionState.Idle || x.State == SessionState.InProgress);
        if (session == null)
        {
            return;
        }

        session.State = SessionState.InProgress;
        await _fileAppDbContext.SaveChangesAsync();

        try
        {
            var deposits = await _financeManagerRestClient.GetDepositsByTransactionIdAsync(session.UserId, null);
            var expenses = await _financeManagerRestClient.GetExpensesByTransactionIdAsync(session.UserId, null);
            var transfers = await _financeManagerRestClient.GetTransfersByTransactionIdAsync(session.UserId, null);
            var transferDepositIds = transfers.Select(x => x.DepositId).ToArray();
            var transferExpenseIds = transfers.Select(x => x.ExpenseId).ToArray();
            var accounts = (await _financeManagerRestClient
                    .GetAccountsByTransactionIdAsync(session.UserId, null))
                .ToDictionary(x => x.Id, x => x);
            var categories = (await _financeManagerRestClient
                    .GetCategoriesByTransactionIdAsync(session.UserId, null))
                .ToDictionary(x => x.Id, x => x.Name);
            var currencies = (await _financeManagerRestClient
                    .GetCurrenciesByTransactionIdAsync(session.UserId, null))
                .ToDictionary(x => x.Id, x => x.ShortName);

            var fileItems = new List<FileContentItem>();
            foreach (var deposit in deposits.Where(d => !transferDepositIds.Contains(d.Id)))
            {
                if (!accounts.ContainsKey(deposit.AccountId)) continue;
                if (!categories.ContainsKey(deposit.CategoryId)) continue;
                var account = accounts[deposit.AccountId];
                if (!currencies.ContainsKey(account.CurrencyId)) continue;
                fileItems.Add(new FileContentItem
                {
                    ToAccountName = account.Name,
                    ToCategoryName = categories[deposit.CategoryId],
                    ToCurrencyName = currencies[account.CurrencyId],
                    ToValue = deposit.Value,
                    Date = deposit.Date
                });
            }

            foreach (var expense in expenses.Where(d => !transferExpenseIds.Contains(d.Id)))
            {
                if (!accounts.ContainsKey(expense.AccountId)) continue;
                if (!categories.ContainsKey(expense.CategoryId)) continue;
                var account = accounts[expense.AccountId];
                if (!currencies.ContainsKey(account.CurrencyId)) continue;
                fileItems.Add(new FileContentItem
                {
                    FromAccountName = account.Name,
                    FromCategoryName = categories[expense.CategoryId],
                    FromCurrencyName = currencies[account.CurrencyId],
                    FromValue = expense.Value,
                    Date = expense.Date
                });
            }

            foreach (var transfer in transfers)
            {
                var deposit = deposits.FirstOrDefault(x => x.Id == transfer.DepositId);
                if (deposit == null) continue;
                if (!accounts.ContainsKey(deposit.AccountId)) continue;
                var depositAccount = accounts[deposit.AccountId];
                if (!currencies.ContainsKey(depositAccount.CurrencyId)) continue;

                var expense = expenses.FirstOrDefault(x => x.Id == transfer.ExpenseId);
                if (expense == null) continue;
                if (!accounts.ContainsKey(expense.AccountId)) continue;
                var expenseAccount = accounts[expense.AccountId];
                if (!currencies.ContainsKey(expenseAccount.CurrencyId)) continue;
                fileItems.Add(new FileContentItem
                {
                    FromAccountName = expenseAccount.Name,
                    FromCurrencyName = currencies[expenseAccount.CurrencyId],
                    FromValue = expense.Value,
                    Date = expense.Date,
                    ToAccountName = depositAccount.Name,
                    ToCurrencyName = currencies[depositAccount.CurrencyId],
                    ToValue = deposit.Value,
                    Description = transfer.Description
                });
            }

            session.FileId = await _sessionFileSerializer.SaveFileAsync(
                session.DateTime,
                fileItems.OrderBy(x => x.Date).ToArray());
            session.State = SessionState.Succeed;
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "Export session with id {ExportSessionId} was failed with error: {ErrorMessage}",
                session.Id,
                e.Message);
            session.State = SessionState.Failed;
        }
        finally
        {
            await _fileAppDbContext.SaveChangesAsync();
        }
    }
}