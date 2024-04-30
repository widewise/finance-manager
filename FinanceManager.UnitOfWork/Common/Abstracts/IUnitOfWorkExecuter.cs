using FinanceManager.UnitOfWork.Abstructs;

namespace FinanceManager.UnitOfWork.Common.Abstracts;

public interface IUnitOfWorkExecuter
{
    void Execute<TRepository>(
        DbConnectionType connectionType,
        string connectionString,
        Action<TRepository> callRepositoryAction)
        where TRepository : IUnitOfWorkRepository;

    TResult Execute<TRepository, TResult>(
        DbConnectionType connectionType,
        string connectionString,
        Func<TRepository, TResult> callRepositoryFunc)
        where TRepository : IUnitOfWorkRepository;
}