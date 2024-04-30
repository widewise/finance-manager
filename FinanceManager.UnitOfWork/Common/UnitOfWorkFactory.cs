using FinanceManager.UnitOfWork.Abstructs;
using FinanceManager.UnitOfWork.Common.Abstracts;

namespace FinanceManager.UnitOfWork.Common;

internal class UnitOfWorkFactory : IUnitOfWorkFactory
{
    public IUnitOfWork CreateUnitOfWork(DbConnectionType type, string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("ConnectionString cannot be null or whitespace");
        }

        var connection = DbConnectionFactory.CreateDbConnection(type, connectionString);

        connection.Open();

        return new UnitOfWork(connection);
    }
}