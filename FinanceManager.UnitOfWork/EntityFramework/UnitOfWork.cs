using System.Collections;
using FinanceManager.UnitOfWork.Abstructs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceManager.UnitOfWork.EntityFramework;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly IServiceProvider _serviceProvider;
    private Hashtable? _repositories;
    private DbContext DbContext { get; set; }
    private IDbContextTransaction? Transaction { get; set; }

    public bool BulkOperation { get; set; }

    internal UnitOfWork(
        DbContext dbContext,
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        DbContext = dbContext;
        Transaction = dbContext.Database.BeginTransaction();
        _repositories = new Hashtable();
    }

    public T Repository<T>() where T : IUnitOfWorkRepository
    {
        var repositoryType = typeof(T);
        var repositoryTypeName = repositoryType.Name;

        if (_repositories!.ContainsKey(repositoryTypeName))
        {
            return (T)_repositories[repositoryTypeName]!;
        }

        var repository = (T)_serviceProvider.GetService(repositoryType)!;

        _repositories.Add(repositoryTypeName, repository);

        return repository;
    }

    public void Commit()
    {
        DbContext.SaveChanges();
        Transaction?.Commit();
        Dispose();
    }

    public void Rollback()
    {
        Transaction?.Rollback();
        Dispose();
    }

    public async Task CommitAsync()
    {
        if (BulkOperation)
        {
            await DbContext.BulkSaveChangesAsync();
        }
        await DbContext.SaveChangesAsync();
        await Transaction?.CommitAsync()!;
        await DisposeAsyncCore();
    }

    public async Task RollbackAsync()
    {
        await Transaction?.RollbackAsync()!;
        await DisposeAsyncCore();
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
            DbContext.Dispose();
            Transaction?.Dispose();
        }
    }

    ~UnitOfWork()
    {
        Dispose(false);
    }

    private async ValueTask DisposeAsyncCore()
    {
        await DbContext.DisposeAsync();

        if (Transaction != null)
        {
            await Transaction.DisposeAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }
}