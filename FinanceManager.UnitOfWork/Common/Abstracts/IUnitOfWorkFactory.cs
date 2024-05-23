using FinanceManager.UnitOfWork.Abstructs;

namespace FinanceManager.UnitOfWork.Common.Abstracts;

public interface IUnitOfWorkFactory
{
    IUnitOfWork CreateUnitOfWork(DbConnectionType type, string connectionString);
}