using FinanceManager.File.Exceptions;
using FinanceManager.File.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Exception = System.Exception;

namespace FinanceManager.File.Services;

public interface IImportDataService
{
    Task ExecuteAsync();
}

public class ImportDataService : IImportDataService
{
    private readonly ILogger<ImportDataService> _logger;
    private readonly FileAppDbContext _fileAppDbContext;
    private readonly ImportDataSettings _settings;
    private readonly ISessionFileSerializer _sessionFileSerializer;
    private readonly IImportCurrenciesService _importCurrenciesService;
    private readonly IImportCategoriesService _importCategoriesService;
    private readonly IImportAccountsService _importAccountsService;
    private readonly IImportMoneyTransactionsService _importMoneyTransactionsService;
    private readonly IImportSessionService _importSessionService;

    public ImportDataService(
        ILogger<ImportDataService> logger,
        FileAppDbContext fileAppDbContext,
        IOptions<ImportDataSettings> settings,
        ISessionFileSerializer sessionFileSerializer,
        IImportCurrenciesService importCurrenciesService,
        IImportCategoriesService importCategoriesService,
        IImportAccountsService importAccountsService,
        IImportMoneyTransactionsService importMoneyTransactionsService,
        IImportSessionService importSessionService)
    {
        _logger = logger;
        _fileAppDbContext = fileAppDbContext;
        _settings = settings.Value;
        _sessionFileSerializer = sessionFileSerializer;
        _importCurrenciesService = importCurrenciesService;
        _importCategoriesService = importCategoriesService;
        _importAccountsService = importAccountsService;
        _importMoneyTransactionsService = importMoneyTransactionsService;
        _importSessionService = importSessionService;
    }

    public async Task ExecuteAsync()
    {
        var session = await _fileAppDbContext.ImportSessions
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
            var (fileItems, fileName) = await _sessionFileSerializer.ParseFileAsync(
                session.FileId);
            var batches = fileItems.Chunk(_settings.BatchSize).ToArray();

            var currencies = new Dictionary<string, Guid>();
            var categories = new Dictionary<string, Guid>();
            var accounts = new Dictionary<string, Guid>();
            foreach (var batch in batches)
            {
                var (checkResult, newState) = await CheckAccessContinueAsync(session.Id);
                if (!checkResult)
                {
                    session.State = newState!.Value;
                    return;
                }
                var resCurrencies = await _importCurrenciesService.ImportAsync(session, batch);
                currencies.Merge(resCurrencies);
                (checkResult, newState) = await CheckAccessContinueAsync(session.Id);
                if (!checkResult)
                {
                    session.State = newState!.Value;
                    return;
                }
                var resCategories = await _importCategoriesService.ImportAsync(
                    session,
                    fileName,
                    batch);
                categories.Merge(resCategories);
                (checkResult, newState) = await CheckAccessContinueAsync(session.Id);
                if (!checkResult)
                {
                    session.State = newState!.Value;
                    return;
                }
                var resAccounts = await _importAccountsService.ImportAsync(
                    session,
                    fileName,
                    currencies,
                    batch);
                accounts.Merge(resAccounts);
                await _importMoneyTransactionsService.ImportAsync(
                    session,
                    categories,
                    accounts,
                    batch);
            }

            session.State = SessionState.Succeed;
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "Import session with id {SessionId} error: {ErrorMessage}",
                session.Id,
                e.Message);
            session.State = SessionState.Failed;
            //TODO: fault tolerance
            await _importSessionService.RejectAsync(session);
            _logger.LogInformation("All data for session with id {SessionId} was rejected", session.Id);
        }
        finally
        {
            await _fileAppDbContext.SaveChangesAsync();
        }
    }

    private async Task<(bool result, SessionState? newState)> CheckAccessContinueAsync(long sessionId)
    {
        var session = await _fileAppDbContext.ImportSessions.FirstOrDefaultAsync(x => x.Id == sessionId);
        if (session == null)
        {
            return (false, null);
        }

        return (session.State != SessionState.Cancelled && session.State != SessionState.Failed, session.State);
    }
}