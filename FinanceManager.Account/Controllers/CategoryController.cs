using System.ComponentModel.DataAnnotations;
using FinanceManager.Account.Models;
using FinanceManager.Account.Services;
using FinanceManager.TransportLibrary;
using FinanceManager.TransportLibrary.Extensions;
using FinanceManager.TransportLibrary.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.Account.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly ILogger<CategoryController> _logger;
    private readonly ICategoryService _categoryService;

    public CategoryController(
        ILogger<CategoryController> logger,
        ICategoryService categoryService)
    {
        _logger = logger;
        _categoryService = categoryService;
    }
    
    [Authorize]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
        [FromHeader(Name = HttpHeaderKeys.RequestId)] string? requestId,
        [FromQuery] Guid? parentId)
    {
        if (!HttpContext.HasUserId(out var currentUserId))
        {
            _logger.LogInformation("The current user identifier is not set");
            return BadRequest("The current user identifier is not set");
        }

        return Ok(await _categoryService.GetAsync(new CategoryQueryParameters
        {
            RequestId = requestId,
            UserId = currentUserId,
            ParentId = parentId
        }));
    }

    [ApiKey]
    [HttpGet("internal")]
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
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(
        [FromHeader(Name = HttpHeaderKeys.RequestId)][Required] string requestId,
        [FromBody] CreateCategoryModel model)
    {
        if (!HttpContext.HasUserId(out var currentUserId))
        {
            _logger.LogInformation("The current user identifier is not set");
            return BadRequest("The current user identifier is not set");
        }

        return Ok(await _categoryService.CreateAsync(model, currentUserId, requestId));
    }

    [ApiKey]
    [HttpPost("{userId}/bulk")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> BulkCreate(
        [FromHeader(Name = HttpHeaderKeys.RequestId)][Required] string requestId,
        [FromRoute] long userId,
        [FromBody] CreateCategoryModel[] models)
    {
        return Ok(await _categoryService.BulkCreateAsync(models, userId, requestId));
    }

    [Authorize]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var res = await _categoryService.DeleteAsync(id);
        return res ? Ok() : BadRequest();
    }

    [ApiKey]
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Reject(
        [FromHeader(Name = HttpHeaderKeys.RequestId)] [Required] string requestId)
    {
        await _categoryService.RejectAsync(requestId);
        return Ok();
    }
}