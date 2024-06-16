using AutoMapper;
using FinanceManager.Account.Domain;
using FinanceManager.Account.Models;
using FinanceManager.Account.Repositories;
using FinanceManager.Events;
using FinanceManager.UnitOfWork.EntityFramework.Abstracts;

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
    private readonly IMapper _mapper;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWorkExecuter _unitOfWorkExecuter;

    public CategoryService(
        ILogger<CategoryService> logger,
        IMapper mapper,
        ICategoryRepository categoryRepository,
        IUnitOfWorkExecuter unitOfWorkExecuter)
    {
        _logger = logger;
        _mapper = mapper;
        _categoryRepository = categoryRepository;
        _unitOfWorkExecuter = unitOfWorkExecuter;
    }

    public async Task<Category[]> GetAsync(CategoryQueryParameters parameters)
    {
        var specification = _mapper.Map<CategorySpecification>(parameters);
        return await _categoryRepository.GetAsync(specification);
    }

    public async Task<Category?> CreateAsync(
        CreateCategoryModel model,
        long userId,
        string requestId)
    {
        if (await _categoryRepository.CheckExistAsync(new CategorySpecification(requestId: requestId)))
        {
            _logger.LogWarning(
                "Category has already created for request with id {RequestId}",
                requestId);
            return null;
        }

        if (await _categoryRepository.CheckExistAsync(new CategorySpecification(userId: userId, name: model.Name)))
        {
            _logger.LogWarning(
                "Category with name {Name} has already created",
                model.Name);
            return null;
        }

        var created = await _unitOfWorkExecuter.ExecuteAsync<ICategoryRepository, Category>(
            repo => repo.CreateAsync(requestId, model.ParentId, model.Type, userId, model.Name, model.Description));

        return created;
    }
    
    public async Task<Category[]?> BulkCreateAsync(
        CreateCategoryModel[] models,
        long userId,
        string requestId)
    {
        if (await _categoryRepository.CheckExistAsync(new CategorySpecification(requestId: requestId)))
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

        await _unitOfWorkExecuter.ExecuteAsync<ICategoryRepository>(
            repo => repo.BulkCreateAsync(newEntities));

        return newEntities;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existed = (await _categoryRepository.GetAsync(new CategorySpecification(id: id))).FirstOrDefault();
        if (existed == null)
        {
            _logger.LogWarning("Category with id {Id} is not found", id);
            return false;
        }

        if (id == TransferConstants.TransferCategoryId || !existed.UserId.HasValue)
        {
            _logger.LogWarning("This is system category");
            return false;
        }

        if (await _categoryRepository.CheckExistAsync(new CategorySpecification(parentId: existed.Id)))
        {
            _logger.LogWarning("Category with id {Id} has children", id);
            return false;
        }

        //TODO: check deposits, expenses
        await _unitOfWorkExecuter.ExecuteAsync<ICategoryRepository>(
            repo => repo.DeleteAsync(existed));
        return true;
    }

    public async Task RejectAsync(string requestId)
    {
        var categories = await _categoryRepository.GetAsync(new CategorySpecification(requestId: requestId));
        if (!categories.Any())
        {
            return;
        }

        await _unitOfWorkExecuter.ExecuteAsync<ICategoryRepository>(
            repo => repo.BulkDeleteAsync(categories));
    }
}