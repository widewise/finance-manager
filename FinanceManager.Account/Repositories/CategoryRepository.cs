using FinanceManager.Account.Domain;
using FinanceManager.Account.Models;
using FinanceManager.UnitOfWork.Abstructs;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.Account.Repositories;

public interface ICategoryRepository : IUnitOfWorkRepository
{
    Task<Category[]> GetAsync(CategorySpecification specification);
    Task<bool> CheckExistAsync(CategorySpecification specification);
    Task<Category> CreateAsync(
        string requestId,
        Guid? parentId,
        CategoryType type,
        long userId,
        string name,
        string? description);
    Task BulkCreateAsync(Category[] entities);
    Task DeleteAsync(Category entity);
    Task BulkDeleteAsync(Category[] entities);
}

public class CategorySpecification
{
    public CategorySpecification() { }

    public CategorySpecification(
        Guid? id = null,
        Guid? parentId = null,
        long? userId = null,
        string? name = null,
        string? requestId = null)
    {
        Id = id;
        ParentId = parentId;
        UserId = userId;
        Name = name;
        RequestId = requestId;
    }

    public Guid? Id { get; set; }
    public Guid? ParentId { get; set; }
    public long? UserId { get; set; }
    public string? Name { get; set; }
    public string? RequestId { get; set; }
}

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _appDbContext;
    private readonly IUnitOfWork _unitOfWork;

    public CategoryRepository(
        AppDbContext appDbContext,
        IUnitOfWork unitOfWork)
    {
        _appDbContext = appDbContext;
        _unitOfWork = unitOfWork;
    }

    private IQueryable<Category> BuildQuery(CategorySpecification specification)
    {
        var query = _appDbContext.Categories.Where(x => true);

        if (specification.Id.HasValue)
        {
            query = query.Where(x => x.Id == specification.Id.Value);
        }

        if (specification.UserId.HasValue)
        {
            query = query.Where(x => x.UserId == specification.UserId.Value);
        }

        if (specification.RequestId != null)
        {
            query = query.Where(x => x.RequestId == specification.RequestId);
        }
        if (specification.Name != null)
        {
            query = query.Where(x => x.Name == specification.Name);
        }

        if (specification.ParentId != null)
        {
            query = query.Where(x => x.ParentId == specification.ParentId);
        }

        return query;
    }

    public async Task<Category[]> GetAsync(CategorySpecification specification)
    {
        var query = BuildQuery(specification);
        return await query.ToArrayAsync();
    }

    public async Task<bool> CheckExistAsync(CategorySpecification specification)
    {
        var query = BuildQuery(specification);
        return await query.AnyAsync();
    }

    public async Task<Category> CreateAsync(string requestId, Guid? parentId, CategoryType type, long userId, string name, string? description)
    {
        var created = await _appDbContext.AddAsync(new Category
        {
            Id = Guid.NewGuid(),
            RequestId = requestId,
            ParentId = parentId,
            Type = type,
            UserId = userId,
            Name = name,
            Description = description
        });
        return created.Entity;
    }

    public Task BulkCreateAsync(Category[] entities)
    {
        _unitOfWork.BulkOperation = true;
        return _appDbContext.BulkInsertAsync(entities);
    }

    public Task DeleteAsync(Category entity)
    {
        _appDbContext.Categories.Remove(entity);
        return Task.CompletedTask;
    }

    public Task BulkDeleteAsync(Category[] entities)
    {
        _appDbContext.Categories.RemoveRange(entities);
        return Task.CompletedTask;
    }
}