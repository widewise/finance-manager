using FinanceManager.UnitOfWork.Abstructs;

namespace FinanceManager.UnitOfWork.EntityFramework.Abstracts;

public interface IUnitOfWorkExecuter
{
    TResult Execute<TResult>(Func<IUnitOfWork, TResult> callRepositoryAction);
    Task<TResult> ExecuteAsync<TResult>(Func<IUnitOfWork, Task<TResult>> callRepositoryAction);
    void Execute<TRepository>(Action<TRepository> callRepositoryAction) where TRepository : IUnitOfWorkRepository;
    Task ExecuteAsync<TRepository>(Func<TRepository, Task> callRepositoryAction) where TRepository : IUnitOfWorkRepository;
    TResult Execute<TRepository, TResult>(Func<TRepository, TResult> callRepositoryFunc) where TRepository : IUnitOfWorkRepository;
    Task<TResult> ExecuteAsync<TRepository, TResult>(Func<TRepository, Task<TResult>> callRepositoryFunc) where TRepository : IUnitOfWorkRepository;
}