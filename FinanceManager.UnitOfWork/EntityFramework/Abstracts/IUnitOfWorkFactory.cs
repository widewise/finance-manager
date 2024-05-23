using FinanceManager.UnitOfWork.Abstructs;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.UnitOfWork.EntityFramework.Abstracts;

public interface IUnitOfWorkFactory
{
    public IUnitOfWork CreateUnitOfWork(DbContext dbContext);
}