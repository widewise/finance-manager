using FinanceManager.UnitOfWork.Abstructs;
using FinanceManager.UnitOfWork.Common.Abstracts;

namespace FinanceManager.UnitOfWork.Common;

internal class DalSessionFactory : IDalSessionFactory
{
    public DalSession CreateSession(DbConnectionType type, string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("ConnectionString cannot be null or whitespace");
        }
        var connection = DbConnectionFactory.CreateDbConnection(type, connectionString);
        return new DalSession(connection);
    }
}