using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using FinanceManager.Deposit.Models;
using FinanceManager.Deposit.Services;
using FinanceManager.Web;
using FinanceManager.Web.Extensions;
using FinanceManager.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.Deposit.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{v:apiVersion}/deposits")]
public class DepositController : ControllerBase
{
    private readonly ILogger<DepositController> _logger;
    private readonly IDepositService _depositService;

    public DepositController(
        ILogger<DepositController> logger,
        IDepositService depositService)
    {
        _logger = logger;
        _depositService = depositService;
    }

    [Authorize]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
    [FromHeader(Name = HttpHeaderKeys.RequestId)] string? requestId,
    [FromQuery] DepositQueryModel? model)
    {
        return Ok(await _depositService.GetAsync(new DepositQueryParameters
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
        return Ok(await _depositService.GetAsync(new DepositQueryParameters
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
        [FromBody] CreateUpdateDepositModel model)
    {
        if (!HttpContext.HasUserId(out var currentUserId))
        {
            _logger.LogInformation(MessageConstants.CurrentUserMessageIsNotSetMessage);
            return BadRequest(MessageConstants.CurrentUserMessageIsNotSetMessage);
        }

        if (!HttpContext.HasUserAddress(out var userAddress))
        {
            _logger.LogInformation("The current user email is not set");
            return BadRequest("The current user email is not set");
        }

        var deposit = await _depositService.CreateAsync(
            requestId,
            currentUserId,
            userAddress,
            model);
        if (deposit == null)
        {
            return BadRequest();
        }
        return Ok(deposit);
    }

    [Authorize]
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        [FromHeader(Name = HttpHeaderKeys.RequestId)] [Required] string requestId,
        [FromRoute] Guid id,
        [FromBody] CreateUpdateDepositModel model)
    {
        if (!HttpContext.HasUserAddress(out var userAddress))
        {
            _logger.LogInformation("The current user email is not set");
            return BadRequest("The current user email is not set");
        }

        var res = await _depositService.UpdateAsync(
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
        var res = await _depositService.DeleteAsync(id);
        return res ? Ok() : NotFound();
    }

    [ApiKey]
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Reject(
        [FromHeader(Name = HttpHeaderKeys.RequestId)] [Required] string requestId)
    {
        await _depositService.RejectAsync(requestId);
        return Ok();
    }
}