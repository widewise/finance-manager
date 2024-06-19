using System.Data;
using Dapper;

namespace FinanceManager.TransportLibrary.Services.Outbox;

public interface IOutboxDbMigrator: IDisposable
{
    void Migrate();
}

public class OutboxDbMigrator : IOutboxDbMigrator
{
    private readonly IDbConnection? _connection;

    public OutboxDbMigrator(IDbConnection connection)
    {
        _connection = connection;
    }
    public void Migrate()
    {
        if (_connection == null)
        {
            throw new InvalidOperationException("DB connection is disposed");
        }

        _connection.Open();
        _connection!.Execute(
            @"CREATE TABLE IF NOT EXISTS public.""OutboxMessages""
(
    ""Id""         uuid                     NOT NULL
        CONSTRAINT ""PK_OutboxMessages""
            PRIMARY KEY,
    ""Type""       text                     NOT NULL,
    ""Content""    text                     NOT NULL,
    ""OccuredAt""  timestamp with time zone NOT NULL,
    ""AttemptCount"" integer                NOT NULL,
    ""ProcessedAt"" timestamp with time zone NULL,
    ""Error""      text                     NULL
);

alter table public.""OutboxMessages"" owner to postgres;
");
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
            _connection?.Dispose();
        }
    }

    ~OutboxDbMigrator()
    {
        Dispose(false);
    }
}