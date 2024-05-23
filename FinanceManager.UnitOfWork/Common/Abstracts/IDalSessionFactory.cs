namespace FinanceManager.UnitOfWork.Common.Abstracts;

public interface IDalSessionFactory
{
    DalSession CreateSession(DbConnectionType type, string connectionString);
}