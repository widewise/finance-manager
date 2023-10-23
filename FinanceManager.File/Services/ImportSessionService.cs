using FinanceManager.File.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.File.Services;

public interface IImportSessionService
{
    Task<long?> CreateAsync(string requestId, long userId, string fileName, Stream fileStream);
    Task<SessionState?> GetStateAsync(long id);
    Task RejectAsync(string requestId);
    Task RejectAsync(ImportSession session);
}

public class ImportSessionService : IImportSessionService
{
    private readonly ILogger<ImportSessionService> _logger;
    private readonly FileAppDbContext _fileAppDbContext;
    private readonly IFinanceManagerRestClient _financeManagerRestClient;

    public ImportSessionService(
        ILogger<ImportSessionService> logger,
        FileAppDbContext fileAppDbContext,
        IFinanceManagerRestClient financeManagerRestClient)
    {
        _logger = logger;
        _fileAppDbContext = fileAppDbContext;
        _financeManagerRestClient = financeManagerRestClient;
    }

    public async Task<long?> CreateAsync(
        string requestId,
        long userId,
        string fileName,
        Stream fileStream)
    {
        if (await _fileAppDbContext.ImportSessions.AnyAsync(x => x.RequestId == requestId))
        {
            _logger.LogWarning(
                "Session has already created for request with id {RequestId}",
                requestId);
            return null;
        }
        var buffer = new byte[fileStream.Length];
        await fileStream.ReadAsync(buffer.AsMemory(start: 0, length: (int)fileStream.Length));

        var createdFile = await _fileAppDbContext.AddAsync(new Models.File
        {
            FileName = fileName,
            FileContent = buffer
        });

        await _fileAppDbContext.SaveChangesAsync();

        var created = await _fileAppDbContext.AddAsync(new ImportSession
        {
            RequestId = requestId,
            UserId = userId,
            FileId = createdFile.Entity.Id,
            DateTime = DateTime.UtcNow,
            State = SessionState.Idle,
        });

        await _fileAppDbContext.SaveChangesAsync();

        return created.Entity.Id;
    }

    public async Task<SessionState?> GetStateAsync(long id)
    {
        var session = await _fileAppDbContext.ImportSessions.FirstOrDefaultAsync(x => x.Id == id);
        if (session == null)
        {
            _logger.LogWarning("Import session with id {ImportSessionId} is not found", id);
            return null;
        }

        return session.State;
    }

    public async Task RejectAsync(string requestId)
    {
        var session = await _fileAppDbContext.ImportSessions.FirstOrDefaultAsync(
            x => x.RequestId == requestId);
        if (session == null)
        {
            _logger.LogWarning("Import session with request id {RequestId} is not found", requestId);
            return;
        }
        session.State = SessionState.Cancelled;
        await RejectAsync(session);
    }

    public async Task RejectAsync(ImportSession session)
    {
        await _financeManagerRestClient.RejectTransfersByTransactionIdAsync(session.RequestId);
        await _financeManagerRestClient.RejectExpensesByTransactionIdAsync(session.RequestId);
        await _financeManagerRestClient.RejectDepositsByTransactionIdAsync(session.RequestId);
        await _financeManagerRestClient.RejectAccountsByTransactionIdAsync(session.RequestId);
        await _financeManagerRestClient.RejectCategoriesByTransactionIdAsync(session.RequestId);
        await _financeManagerRestClient.RejectCurrenciesByTransactionIdAsync(session.RequestId);
        await _fileAppDbContext.SaveChangesAsync();
    }
}