using FinanceManager.UnitOfWork.Abstructs;
using FinanceManager.UnitOfWork.EntityFramework.Abstracts;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.UnitOfWork.EntityFramework;

public class UnitOfWorkExecuter : IUnitOfWorkExecuter
{
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly DbContext _dbContext;

    public UnitOfWorkExecuter(
        IUnitOfWorkFactory unitOfWorkFactory,
        DbContext dbContext)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _dbContext = dbContext;
    }

    public TResult Execute<TResult>(Func<IUnitOfWork, TResult> callRepositoryAction)
    {
        using var unitOfWork = _unitOfWorkFactory.CreateUnitOfWork(_dbContext);
        try
        {
            var result = callRepositoryAction(unitOfWork);
            unitOfWork.Commit();
            return result;
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }

    public async Task<TResult> ExecuteAsync<TResult>(Func<IUnitOfWork, Task<TResult>> callRepositoryAction)
    {
        await using var unitOfWork = _unitOfWorkFactory.CreateUnitOfWork(_dbContext);
        try
        {
            var result = await callRepositoryAction(unitOfWork);
            await unitOfWork.CommitAsync();
            return result;
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }

    public void Execute<TRepository>(
        Action<TRepository> callRepositoryAction)
        where TRepository : IUnitOfWorkRepository
    {
        Execute(unitOfWork =>
        {
            var repository = unitOfWork.Repository<TRepository>();

            callRepositoryAction(repository);
            return 0;
        });
    }

    public async Task ExecuteAsync<TRepository>(Func<TRepository, Task> callRepositoryAction)
        where TRepository : IUnitOfWorkRepository
    {
        await ExecuteAsync( async unitOfWork =>
        {
            var repository = unitOfWork.Repository<TRepository>();
            await callRepositoryAction(repository);
            return 0;
        });
    }

    public TResult Execute<TRepository, TResult>(
        Func<TRepository, TResult> callRepositoryFunc)
        where TRepository : IUnitOfWorkRepository
    {
        return Execute<TResult>(unitOfWork =>
        {
            var repository = unitOfWork.Repository<TRepository>();
            return callRepositoryFunc(repository);
        });
    }

    public async Task<TResult> ExecuteAsync<TRepository, TResult>(Func<TRepository, Task<TResult>> callRepositoryFunc) where TRepository : IUnitOfWorkRepository
    {
        return await ExecuteAsync<TResult>(async unitOfWork =>
        {
            var repository = unitOfWork.Repository<TRepository>();
            return await callRepositoryFunc(repository);
        });
    }
}