using System.ComponentModel.DataAnnotations;
using FinanceManager.Account.Models;
using FinanceManager.Account.Services;
using FinanceManager.TransportLibrary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.Account.Controllers;

[Authorize]
[Route("api/[controller]")]
public class AccountLimitController : BaseController
{
    private readonly IAccountLimitService _accountLimitService;

    public AccountLimitController(
        IAccountLimitService accountLimitService)
    {
        _accountLimitService = accountLimitService;
    }

    [HttpGet("{accountId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromRoute] Guid accountId)
    {
        return Ok(await _accountLimitService.GetAsync(accountId));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(
        [FromHeader(Name = HttpHeaderKeys.RequestId)] [Required]
        string requestId,
        [FromBody] CreateAccountLimitModel model)
    {
        return Ok(await _accountLimitService.CreateAsync(model, requestId));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateAccountLimitModel model)
    {
        var res = await _accountLimitService.UpdateAsync(id, model);
        return res ? Ok() : NotFound();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var res = await _accountLimitService.DeleteAsync(id);
        return res ? Ok() : BadRequest();
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