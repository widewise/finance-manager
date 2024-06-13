using System.Data;
using Dapper;
using FinanceManager.TransportLibrary.Models;

namespace FinanceManager.TransportLibrary.Services.Outbox;

public interface IMessageRepository
{
    Task<Message[]> GetRecentAsync(int limit);
    Task AddAsync(string type, string? content);
    Task UpdateAsync(Message message);
}

public class MessageRepository : IMessageRepository, IAsyncDisposable
{
    private readonly IDbConnection? _dbConnection;

    public MessageRepository(
        IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task AddAsync(string type, string? content)
    {
        CheckDbConnection();
        await _dbConnection!.ExecuteAsync(
            @"INSERT INTO ""OutboxMessages""(""Id"", ""OccuredAt"", ""Type"", ""Content"", ""AttemptCount"") VALUES (@Id, @OccuredAt, @Type, @Content, @AttemptCount)",
            new
            {
                Id = Guid.NewGuid(),
                OccuredAt = DateTime.UtcNow,
                Type = type,
                Content = content,
                AttemptCount = 0
            });
    }

    public async Task UpdateAsync(Message message)
    {
        CheckDbConnection();
        await _dbConnection!.ExecuteAsync(
            @"UPDATE ""OutboxMessages"" SET ""ProcessedAt""=@ProcessedAt, ""Error""=@Error, ""AttemptCount""=@AttemptCount WHERE ""Id""=@Id",
            new
            {
                message.Id,
                message.ProcessedAt,
                message.Error,
                message.AttemptCount
            });
    }

    public async Task<Message[]> GetRecentAsync(int limit)
    {
        CheckDbConnection();
        return (await _dbConnection!.QueryAsync<Message>(
                @"SELECT ""Id"", ""Type"", ""Content"", ""OccuredAt"", ""ProcessedAt"", ""Error"" FROM ""OutboxMessages"" where ""ProcessedAt"" IS NULL ORDER BY ""OccuredAt"" DESC LIMIT @MessageCount",
                new
                {
                    MessageCount = limit
                }))
            .ToArray();
    }

    private void CheckDbConnection()
    {
        if (_dbConnection == null)
        {
            throw new InvalidOperationException("DB connection is disposed");
        }

        try
        {
            _dbConnection.Open();
        }
        catch
        {
            //ignore
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _dbConnection?.Dispose();
        }
    }

    ~MessageRepository()
    {
        Dispose(false);
    }

    private async ValueTask DisposeAsyncCore()
    {
        if (_dbConnection != null)
        {
            await CastAndDispose(_dbConnection);
        }

        return;

        static async ValueTask CastAndDispose(IDisposable resource)
        {
            if (resource is IAsyncDisposable resourceAsyncDisposable)
                await resourceAsyncDisposable.DisposeAsync();
            else
                resource.Dispose();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }
}