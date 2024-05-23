using System.Collections;
using System.Data;
using FinanceManager.UnitOfWork.Abstructs;

namespace FinanceManager.UnitOfWork.Common;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly Hashtable? _repositories;
    public IDbConnection? Connection { get; private set; }
    public IDbTransaction? Transaction { get; private set; }

    public bool BulkOperation { get; set; }

    internal UnitOfWork(IDbConnection connection)
    {
        if (connection.State == ConnectionState.Open)
        {
            throw new ArgumentException("connection.State == ConnectionState.Open");
        }
        Connection = connection;
        Transaction = Connection.BeginTransaction();
        _repositories = new Hashtable();
    }

    public T Repository<T>()
        where T : IUnitOfWorkRepository
    {
        var repositoryType = typeof(T);
        var repositoryTypeName = repositoryType.Name;

        if (_repositories!.ContainsKey(repositoryTypeName))
        {
            return (T)_repositories[repositoryTypeName]!;
        }

        var repository = (T)Activator.CreateInstance(repositoryType, Transaction)!;

        _repositories.Add(repositoryTypeName, repository);

        return repository;
    }

    public void Commit()
    {
        Transaction?.Commit();
        Dispose();
    }

    public Task CommitAsync()
    {
        Transaction?.Commit();
        Dispose();

        return Task.CompletedTask;
    }

    public void Rollback()
    {
        Transaction?.Rollback();
        Dispose();
    }

    public Task RollbackAsync()
    {
        Transaction?.Rollback();
        Dispose();

        return Task.CompletedTask;
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
            Connection?.Dispose();
            Transaction?.Dispose();
        }
    }

    ~UnitOfWork()
    {
        Dispose(false);
    }

    private async ValueTask DisposeAsyncCore()
    {
        if (Connection != null)
        {
            await CastAndDispose(Connection);
        }

        if (Transaction != null)
        {
            await CastAndDispose(Transaction);
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