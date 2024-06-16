using FinanceManager.UnitOfWork.Abstructs;
using Microsoft.EntityFrameworkCore;
using IUnitOfWorkFactory = FinanceManager.UnitOfWork.EntityFramework.Abstracts.IUnitOfWorkFactory;

namespace FinanceManager.UnitOfWork.EntityFramework;

internal class UnitOfWorkFactory : IUnitOfWorkFactory, IAsyncDisposable, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private IUnitOfWork? _unitOfWork;

    public UnitOfWorkFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IUnitOfWork CreateUnitOfWork(DbContext dbContext)
    {
        if (_unitOfWork == null)
        {
            _unitOfWork = new UnitOfWork(dbContext, _serviceProvider);
        }

        return _unitOfWork;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _unitOfWork?.Dispose();
        }
    }

    ~UnitOfWorkFactory()
    {
        Dispose(false);
    }

    private async ValueTask DisposeAsyncCore()
    {
        if (_unitOfWork != null)
        {
            await _unitOfWork.DisposeAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }
}