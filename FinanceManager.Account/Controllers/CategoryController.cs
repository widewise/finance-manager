using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using AutoMapper;
using FinanceManager.Account.Domain;
using FinanceManager.Account.Models;
using FinanceManager.Account.Services;
using FinanceManager.Web;
using FinanceManager.Web.Extensions;
using FinanceManager.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.Account.Controllers;

[ApiVersion(1.0)]
[Route("api/v{v:apiVersion}/categories")]
public class CategoryController : BaseController
{
    private readonly ILogger<CategoryController> _logger;
    private readonly ICategoryService _categoryService;
    private readonly IMapper _mapper;
    private readonly ILinkService _linkService;

    public CategoryController(
        ILogger<CategoryController> logger,
        ICategoryService categoryService,
        IMapper mapper,
        ILinkService linkService)
    {
        _logger = logger;
        _categoryService = categoryService;
        _mapper = mapper;
        _linkService = linkService;
    }
    
    [Authorize]
    [HttpGet(Name = ApiNameConstants.GetCategory)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
        [FromHeader(Name = HttpHeaderKeys.RequestId)] string? requestId,
        [FromQuery] Guid? parentId)
    {
        if (!HttpContext.HasUserId(out var currentUserId))
        {
            _logger.LogInformation(MessageConstants.CurrentUserMessageIsNotSetMessage);
            return BadRequest(MessageConstants.CurrentUserMessageIsNotSetMessage);
        }

        var categories = await _categoryService.GetAsync(new CategoryQueryParameters
        {
            RequestId = requestId,
            UserId = currentUserId,
            ParentId = parentId
        });

        return Ok(categories.Select(GetResponse));
    }

    [ApiKey]
    [HttpGet("internal", Name = ApiNameConstants.GetInternalCategory)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInternal(
        [FromHeader(Name = HttpHeaderKeys.RequestId)] string? requestId,
        [FromQuery] long? userId,
        [FromQuery] Guid? parentId)
    {
        return Ok(await _categoryService.GetAsync(new CategoryQueryParameters
        {
            RequestId = requestId,
            UserId = userId,
            ParentId = parentId
        }));
    }

    [Authorize]
    [HttpPost(Name = ApiNameConstants.CreateCategory)]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromHeader(Name = HttpHeaderKeys.RequestId)][Required] string requestId,
        [FromBody] CreateCategoryModel model)
    {
        if (!HttpContext.HasUserId(out var currentUserId))
        {
            _logger.LogInformation(MessageConstants.CurrentUserMessageIsNotSetMessage);
            return BadRequest(MessageConstants.CurrentUserMessageIsNotSetMessage);
        }

        var category = await _categoryService.CreateAsync(model, currentUserId, requestId);
        if (category == null)
        {
            return BadRequest();
        }

        var response = GetResponse(category);
        return Created(response.Links.FirstOrDefault()?.Href ?? string.Empty, response);
    }

    [ApiKey]
    [HttpPost("{userId}/bulk", Name = ApiNameConstants.BulkCreateCategories)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> BulkCreate(
        [FromHeader(Name = HttpHeaderKeys.RequestId)][Required] string requestId,
        [FromRoute] long userId,
        [FromBody] CreateCategoryModel[] models)
    {
        return Ok(await _categoryService.BulkCreateAsync(models, userId, requestId));
    }

    [Authorize]
    [HttpDelete("{id}", Name = ApiNameConstants.DeleteCategory)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var res = await _categoryService.DeleteAsync(id);
        return res ? Ok() : BadRequest();
    }

    [ApiKey]
    [HttpDelete(Name = ApiNameConstants.RejectCategories)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Reject(
        [FromHeader(Name = HttpHeaderKeys.RequestId)] [Required] string requestId)
    {
        await _categoryService.RejectAsync(requestId);
        return Ok();
    }
    
    [HttpOptions]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [AllowAnonymous]
    public IActionResult DocumentsOptions()
    {
        AddAllowHeader();
        return Ok();
    }
        
    private CategoryResponse GetResponse(Category category)
    {
        var response = _mapper.Map<CategoryResponse>(category);
        var id = response.Id;
        var parentId = response.ParentId;
        var links = new List<Link>();

        links.Add(_linkService.Generate(
            ApiNameConstants.GetCategory,
            new { parentId },
            ApiNameConstants.Self,
            HttpMethod.Get.Method));

        links.Add(_linkService.Generate(
            ApiNameConstants.DeleteCategory,
            new { id },
            ApiNameConstants.DeleteCategoryRel,
            HttpMethod.Delete.Method));

        response.Links = links.ToArray();
        return response;
    }
}