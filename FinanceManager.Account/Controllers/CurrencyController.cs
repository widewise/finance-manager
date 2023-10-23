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
public class CurrencyController : ControllerBase
{
    private readonly ILogger<CurrencyController> _logger;
    private readonly ICurrencyService _currencyService;

    public CurrencyController(
        ILogger<CurrencyController> logger,
        ICurrencyService currencyService)
    {
        _logger = logger;
        _currencyService = currencyService;
    }
    
    [Authorize]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
        [FromHeader(Name = HttpHeaderKeys.RequestId)] string? requestId)
    {
        if (!HttpContext.HasUserId(out var currentUserId))
        {
            _logger.LogInformation("The current user identifier is not set");
            return BadRequest("The current user identifier is not set");
        }

        return Ok(await _currencyService.GetAsync(new CurrencyQueryParameters
        {
            RequestId = requestId,
            UserId = currentUserId
        }));
    }

    [ApiKey]
    [HttpGet("internal")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInternal(
        [FromHeader(Name = HttpHeaderKeys.RequestId)] string? requestId,
        [FromQuery] long? userId)
    {
        return Ok(await _currencyService.GetAsync(new CurrencyQueryParameters
        {
            RequestId = requestId,
            UserId = userId
        }));
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(
        [FromHeader(Name = HttpHeaderKeys.RequestId)][Required] string requestId,
        [FromBody] CreateCurrencyModel model)
    {
        if (!HttpContext.HasUserId(out var currentUserId))
        {
            _logger.LogInformation("The current user identifier is not set");
            return BadRequest("The current user identifier is not set");
        }

        var currency = await _currencyService.CreateAsync(requestId, currentUserId, model);
        return currency != null ? Ok(currency) : BadRequest();
    }

    [ApiKey]
    [HttpPost("{userId}/bulk")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> BulkCreate(
        [FromHeader(Name = HttpHeaderKeys.RequestId)][Required] string requestId,
        [FromRoute] long userId,
        [FromBody] CreateCurrencyModel[] models)
    {
        var currencies = await _currencyService.BulkCreateAsync(models, userId, requestId);
        return currencies != null ? Ok(currencies) : BadRequest();
    }

    [Authorize]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        if (!HttpContext.HasUserId(out var currentUserId))
        {
            _logger.LogInformation("The current user identifier is not set");
            return BadRequest("The current user identifier is not set");
        }

        var res = await _currencyService.DeleteAsync(id, currentUserId);
        return res ? Ok() : BadRequest();
    }

    [ApiKey]
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Reject(
        [FromHeader(Name = HttpHeaderKeys.RequestId)] [Required] string requestId)
    {
        await _currencyService.RejectAsync(requestId);
        return Ok();
    }
}