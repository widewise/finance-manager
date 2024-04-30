using System.Data;
using FinanceManager.UnitOfWork.Abstructs;

namespace FinanceManager.UnitOfWork.Common;

public class DalSession : IDisposable
{
    private readonly IDbConnection _connection;

    public IUnitOfWork UnitOfWork { get; }

    internal DalSession(IDbConnection connection)
    {
        _connection = connection;
        _connection.Open();
        UnitOfWork = new UnitOfWork(connection);
    }

    public void Dispose()
    {
        UnitOfWork.Dispose();
        _connection.Dispose();
    }
}