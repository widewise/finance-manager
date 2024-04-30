using FinanceManager.UnitOfWork.Abstructs;
using Microsoft.EntityFrameworkCore;
using IUnitOfWorkFactory = FinanceManager.UnitOfWork.EntityFramework.Abstracts.IUnitOfWorkFactory;

namespace FinanceManager.UnitOfWork.EntityFramework;

internal class UnitOfWorkFactory : IUnitOfWorkFactory
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
}