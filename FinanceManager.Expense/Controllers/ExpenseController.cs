using System.ComponentModel.DataAnnotations;
using FinanceManager.Expense.Models;
using FinanceManager.Expense.Services;
using FinanceManager.TransportLibrary;
using FinanceManager.TransportLibrary.Extensions;
using FinanceManager.TransportLibrary.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.Expense.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpenseController : ControllerBase
{
    private readonly ILogger<ExpenseController> _logger;
    private readonly IExpenseService _expenceService;

    public ExpenseController(
        ILogger<ExpenseController> logger,
        IExpenseService expenceService)
    {
        _logger = logger;
        _expenceService = expenceService;
    }

    [Authorize]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
        [FromHeader(Name = HttpHeaderKeys.RequestId)] string? requestId,
        [FromQuery] ExpenseQueryModel? model)
    {
        return Ok(await _expenceService.GetAsync(new ExpenseQueryParameters
        {
            RequestId = requestId,
            Id = model?.Id,
            AccountId = model?.AccountId,
            CategoryId = model?.CategoryId,
            Date = model?.Date
        }));
    }

    [ApiKey]
    [HttpGet("internal")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
        [FromHeader(Name = HttpHeaderKeys.RequestId)] string? requestId,
        [FromQuery] long? userId)
    {
        return Ok(await _expenceService.GetAsync(new ExpenseQueryParameters
        {
            RequestId = requestId,
            UserId = userId
        }));
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(
        [FromHeader(Name = HttpHeaderKeys.RequestId)] [Required] string requestId,
        [FromBody] CreateUpdateExpenseModel model)
    {
        if (!HttpContext.HasUserId(out var currentUserId))
        {
            _logger.LogInformation("The current user identifier is not set");
            return BadRequest("The current user identifier is not set");
        }

        if (!HttpContext.HasUserAddress(out var userAddress))
        {
            _logger.LogInformation("The current user email is not set");
            return BadRequest("The current user email is not set");
        }

        return Ok(await _expenceService.CreateAsync(
            requestId,
            currentUserId,
            userAddress,
            model));
    }

    [Authorize]
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        [FromHeader(Name = HttpHeaderKeys.RequestId)] [Required] string requestId,
        [FromRoute] Guid id,
        [FromBody] CreateUpdateExpenseModel model)
    {
        if (!HttpContext.HasUserAddress(out var userAddress))
        {
            _logger.LogInformation("The current user email is not set");
            return BadRequest("The current user email is not set");
        }

        var res = await _expenceService.UpdateAsync(
            id,
            requestId,
            userAddress,
            model);
        return res ? Ok() : NotFound();
    }

    [Authorize]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var res = await _expenceService.DeleteAsync(id);
        return res ? Ok() : NotFound();
    }

    [ApiKey]
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Reject(
        [FromHeader(Name = HttpHeaderKeys.RequestId)] [Required] string requestId)
    {
        await _expenceService.RejectAsync(requestId);
        return Ok();
    }
}