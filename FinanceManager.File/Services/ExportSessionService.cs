using FinanceManager.File.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.File.Services;

public interface IExportSessionService
{
    Task<long?> CreateAsync(long userId, string requestId);
    Task<SessionState?> GetStateAsync(long id);
    Task<Stream?> DownloadAsync(long id);
}

public class ExportSessionService : IExportSessionService
{
    private readonly ILogger<ExportSessionService> _logger;
    private readonly FileAppDbContext _fileAppDbContext;

    public ExportSessionService(
        ILogger<ExportSessionService> logger,
        FileAppDbContext fileAppDbContext)
    {
        _logger = logger;
        _fileAppDbContext = fileAppDbContext;
    }
    public async Task<long?> CreateAsync(long userId, string requestId)
    {
        if (await _fileAppDbContext.ExportSessions.AnyAsync(x => x.RequestId == requestId))
        {
            _logger.LogWarning(
                "Session has already created for request with id {RequestId}",
                requestId);
            return null;
        }

        var created = await _fileAppDbContext.AddAsync(new ExportSession
        {
            RequestId = requestId,
            UserId = userId,
            State = SessionState.Idle,
            DateTime = DateTime.UtcNow
        });

        await _fileAppDbContext.SaveChangesAsync();

        return created.Entity.Id;
    }

    public async Task<SessionState?> GetStateAsync(long id)
    {
        var session = await _fileAppDbContext.ExportSessions.FirstOrDefaultAsync(x => x.Id == id);
        if (session == null)
        {
            _logger.LogWarning("Export session with id {ExportSessionId} is not found", id);
            return null;
        }

        return session.State;
    }

    public async Task<Stream?> DownloadAsync(long id)
    {
        var session = await _fileAppDbContext.ExportSessions.FirstOrDefaultAsync(x => x.Id == id);
        if (session == null)
        {
            _logger.LogWarning("Export session with id {ExportSessionId} is not found", id);
            return null;
        }

        if (session.State != SessionState.Succeed || !session.FileId.HasValue)
        {
            _logger.LogWarning("Export session with id {ExportSessionId} is not completed", id);
            return null;
        }

        var file = await _fileAppDbContext.Files.FirstOrDefaultAsync(x => x.Id == session.FileId);
        if (file == null)
        {
            _logger.LogWarning("File with id {FileId} is not found", id);
            return null;
        }

        return new MemoryStream(file.FileContent);
    }
}