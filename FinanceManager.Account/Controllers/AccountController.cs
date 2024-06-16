using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using FinanceManager.Account.Models;
using FinanceManager.Account.Services;
using FinanceManager.Web;
using FinanceManager.Web.Extensions;
using FinanceManager.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.Account.Controllers;

[ApiVersion(1.0)]
[Route("api/v{v:apiVersion}/accounts")]
public class AccountController : BaseController
{
    private readonly ILogger<AccountController> _logger;
    private readonly IAccountService _accountService;

    public AccountController(
        ILogger<AccountController> logger,
        IAccountService accountService)
    {
        _logger = logger;
        _accountService = accountService;
    }

    [Authorize]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
        [FromHeader(Name = HttpHeaderKeys.RequestId)] string? requestId,
        [FromQuery] Guid? id)
    {
        if (!HttpContext.HasUserId(out var currentUserId))
        {
            _logger.LogInformation(MessageConstants.CurrentUserMessageIsNotSetMessage);
            return BadRequest(MessageConstants.CurrentUserMessageIsNotSetMessage);
        }

        return Ok(await _accountService.GetAsync(new AccountQueryParameters
        {
            RequestId = requestId,
            UserId = currentUserId,
            Id = id
        }));
    }

    [ApiKey]
    [HttpGet("internal")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInternal(
        [FromHeader(Name = HttpHeaderKeys.RequestId)] string? requestId,
        [FromQuery] Guid? id,
        [FromQuery] long? userId)
    {
        return Ok(await _accountService.GetAsync(new AccountQueryParameters
        {
            RequestId = requestId,
            UserId = userId,
            Id = id
        }));
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(
        [FromHeader(Name = HttpHeaderKeys.RequestId)] [Required] string requestId,
        [FromBody] CreateAccountModel model)
    {
        if (!HttpContext.HasUserId(out var currentUserId))
        {
            _logger.LogInformation(MessageConstants.CurrentUserMessageIsNotSetMessage);
            return BadRequest(MessageConstants.CurrentUserMessageIsNotSetMessage);
        }

        return Ok(await _accountService.CreateAsync(model, currentUserId, requestId));
    }

    [ApiKey]
    [HttpPost("{userId}/bulk")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> BulkCreate(
        [FromHeader(Name = HttpHeaderKeys.RequestId)] [Required] string requestId,
        [FromRoute] long userId,
        [FromBody] CreateAccountModel[] models)
    {
        return Ok(await _accountService.BulkCreateAsync(models, userId, requestId));
    }

    [Authorize]
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateAccountModel model)
    {
        var res = await _accountService.UpdateAsync(id, model);
        return res ? Ok() : NotFound();
    }

    [Authorize]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var res = await _accountService.DeleteAsync(id);
        return res ? Ok() : BadRequest();
    }

    [ApiKey]
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Reject(
        [FromHeader(Name = HttpHeaderKeys.RequestId)] [Required] string requestId)
    {
        await _accountService.RejectAsync(requestId);
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
}