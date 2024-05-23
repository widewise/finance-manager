using System.Data;

namespace FinanceManager.UnitOfWork.Abstructs;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    bool BulkOperation { get; set; }
    T Repository<T>() where T : IUnitOfWorkRepository;
    void Commit();
    void Rollback();
    Task CommitAsync();
    Task RollbackAsync();
}