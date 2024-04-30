using FinanceManager.UnitOfWork.Abstructs;
using FinanceManager.UnitOfWork.Common.Abstracts;

namespace FinanceManager.UnitOfWork.Common;

public class UnitOfWorkExecuter : IUnitOfWorkExecuter
{
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;

    public UnitOfWorkExecuter(IUnitOfWorkFactory unitOfWorkFactory)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public void Execute<TRepository>(
        DbConnectionType connectionType,
        string connectionString,
        Action<TRepository> callRepositoryAction)
        where TRepository : IUnitOfWorkRepository
    {
        using var unitOfWork = _unitOfWorkFactory.CreateUnitOfWork(connectionType, connectionString);
        try
        {
            var repository = unitOfWork.Repository<TRepository>();

            callRepositoryAction(repository);

            unitOfWork.Commit();
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }

    public TResult Execute<TRepository, TResult>(
        DbConnectionType connectionType,
        string connectionString,
        Func<TRepository, TResult> callRepositoryFunc)
        where TRepository : IUnitOfWorkRepository
    {
        using var unitOfWork = _unitOfWorkFactory.CreateUnitOfWork(connectionType, connectionString);
        try
        {
            var repository = unitOfWork.Repository<TRepository>();

            var result = callRepositoryFunc(repository);

            unitOfWork.Commit();

            return result;
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }
}