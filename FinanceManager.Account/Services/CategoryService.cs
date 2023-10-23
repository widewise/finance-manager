using FinanceManager.Account.Models;
using FinanceManager.Events;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.Account.Services;

public interface ICategoryService
{
    Task<Category[]> GetAsync(CategoryQueryParameters parameters);
    Task<Category?> CreateAsync(
        CreateCategoryModel model,
        long userId,
        string requestId);
    Task<Category[]?> BulkCreateAsync(
        CreateCategoryModel[] models,
        long userId,
        string requestId);
    Task<bool> DeleteAsync(Guid id);
    Task RejectAsync(string requestId);
}

public class CategoryService: ICategoryService
{
    private readonly ILogger<CategoryService> _logger;
    private readonly AppDbContext _appDbContext;

    public CategoryService(
        ILogger<CategoryService> logger,
        AppDbContext appDbContext)
    {
        _logger = logger;
        _appDbContext = appDbContext;
    }

    public async Task<Category[]> GetAsync(CategoryQueryParameters parameters)
    {
        var query = _appDbContext.Categories.Where(x => true);

        if (parameters.UserId.HasValue)
        {
            query = query.Where(x => x.UserId == parameters.UserId.Value);
        }

        if (parameters.RequestId != null)
        {
            query = query.Where(x => x.RequestId == parameters.RequestId);
        }

        if (parameters.ParentId != null)
        {
            query = query.Where(x => x.ParentId == parameters.ParentId);
        }
            
        return await query.ToArrayAsync();
    }

    public async Task<Category?> CreateAsync(
        CreateCategoryModel model,
        long userId,
        string requestId)
    {
        if (await _appDbContext.Categories.AnyAsync(x => x.RequestId == requestId))
        {
            _logger.LogWarning(
                "Category has already created for request with id {RequestId}",
                requestId);
            return null;
        }

        if (await _appDbContext.Categories.AnyAsync(
                x => x.Name == model.Name && x.UserId == userId))
        {
            _logger.LogWarning(
                "Category with name {Name} has already created",
                model.Name);
            return null;
        }

        var created = await _appDbContext.AddAsync(new Category
        {
            Id = Guid.NewGuid(),
            RequestId = requestId,
            ParentId = model.ParentId,
            Type = model.Type,
            UserId = userId,
            Name = model.Name,
            Description = model.Description
        });
        
        await _appDbContext.SaveChangesAsync();

        return created.Entity;
    }
    
    public async Task<Category[]?> BulkCreateAsync(
        CreateCategoryModel[] models,
        long userId,
        string requestId)
    {
        if (await _appDbContext.Categories.AnyAsync(x => x.RequestId == requestId))
        {
            _logger.LogWarning(
                "Categories has already created for request with id {RequestId}",
                requestId);
            return null;
        }

        //TODO: saving with parent id
        var newEntities = models.Select(model => new Category
        {
            Id = Guid.NewGuid(),
            RequestId = requestId,
            UserId = userId,
            Name = model.Name,
            Type = model.Type,
            Description = model.Description
        }).ToArray();

        await _appDbContext.BulkInsertAsync(newEntities);
        await _appDbContext.BulkSaveChangesAsync();

        return newEntities;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existed = await _appDbContext.Categories.FirstOrDefaultAsync(x => x.Id == id);
        if (existed == null)
        {
            _logger.LogWarning("Category with id {d} is not found", id);
            return false;
        }

        if (id == TransferConstants.TransferCategoryId || !existed.UserId.HasValue)
        {
            _logger.LogWarning("This is system category");
            return false;
        }

        if (await _appDbContext.Categories.AnyAsync(x => x.ParentId == existed.Id))
        {
            _logger.LogWarning("Category with id {d} has children", id);
            return false;
        }

        //TODO: check deposits, expenses
        _appDbContext.Categories.Remove(existed);
        await _appDbContext.SaveChangesAsync();
        return true;
    }

    public async Task RejectAsync(string requestId)
    {
        var categories = await _appDbContext.Categories
            .Where(x => x.RequestId == requestId)
            .ToArrayAsync();
        if (!categories.Any())
        {
            return;
        }

        _appDbContext.Categories.RemoveRange(categories);
        await _appDbContext.SaveChangesAsync();
    }
}