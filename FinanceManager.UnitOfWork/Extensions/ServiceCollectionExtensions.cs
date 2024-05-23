using FinanceManager.UnitOfWork.Abstructs;
using FinanceManager.UnitOfWork.Common;
using FinanceManager.UnitOfWork.Common.Abstracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UnitOfWorkExecuter = FinanceManager.UnitOfWork.EntityFramework.UnitOfWorkExecuter;
using UnitOfWorkFactory = FinanceManager.UnitOfWork.Common.UnitOfWorkFactory;

namespace FinanceManager.UnitOfWork.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommonUnitOfWork(this IServiceCollection services)
    {
        services.AddScoped<IDalSessionFactory, DalSessionFactory>();
        services.AddScoped<IUnitOfWorkFactory, UnitOfWorkFactory>();
        services.AddTransient<IUnitOfWorkExecuter, Common.UnitOfWorkExecuter>();

        return services;
    }
    public static IServiceCollection AddEntityFrameworkUnitOfWork(this IServiceCollection services)
    {
        services.AddScoped<EntityFramework.Abstracts.IUnitOfWorkFactory, EntityFramework.UnitOfWorkFactory>();
        services.AddScoped<IUnitOfWork>(sp =>
        {
            var factory = sp.GetRequiredService<EntityFramework.Abstracts.IUnitOfWorkFactory>();
            var dbContext = sp.GetRequiredService<DbContext>();
            return factory.CreateUnitOfWork(dbContext);
        });
        services.AddTransient<EntityFramework.Abstracts.IUnitOfWorkExecuter, UnitOfWorkExecuter>();

        return services;
    }
}