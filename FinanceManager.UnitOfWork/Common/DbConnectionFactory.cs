using System.Data;
using System.Data.SqlClient;
using Npgsql;

namespace FinanceManager.UnitOfWork.Common;

internal static class DbConnectionFactory
{
    public static IDbConnection CreateDbConnection(DbConnectionType type, string connectionString)
    {
        return type switch
        {
            DbConnectionType.Npgsql => new NpgsqlConnection(connectionString),
            DbConnectionType.SqlClient => new SqlConnection(connectionString),
            _ => throw new NotSupportedException(nameof(type))
        };
    }
}