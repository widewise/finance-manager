using System.ComponentModel.DataAnnotations;
using FinanceManager.Transfer.Models;
using FinanceManager.Transfer.Services;
using FinanceManager.TransportLibrary;
using FinanceManager.TransportLibrary.Extensions;
using FinanceManager.TransportLibrary.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.Transfer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransferController : ControllerBase
{
    private readonly ILogger<TransferController> _logger;
    private readonly ITransferService _transferService;

    public TransferController(
        ILogger<TransferController> logger,
        ITransferService transferService)
    {
        _logger = logger;
        _transferService = transferService;
    }

    [Authorize]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
        [FromHeader(Name = HttpHeaderKeys.RequestId)] string? requestId,
        [FromQuery] Guid? id)
    {
        return Ok(await _transferService.GetAsync(new TransferQueryParameters
        {
            RequestId = requestId,
            Id = id
        }));
    }

    [ApiKey]
    [HttpGet("internal")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInternal(
        [FromHeader(Name = HttpHeaderKeys.RequestId)] string? requestId,
        [FromQuery] long? userId)
    {
        return Ok(await _transferService.GetAsync(new TransferQueryParameters
        {
            RequestId = requestId,
            UserId = userId
        }));
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(
        [FromHeader(Name = HttpHeaderKeys.RequestId)] [Required]
        string requestId,
        [FromBody] CreateTransferModel model)
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

        var transfer = await _transferService.CreateAsync(
            requestId,
            currentUserId,
            userAddress,
            model);
        if (transfer == null)
        {
            return BadRequest();
        }
        return Ok(transfer);
    }

    [Authorize]
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateTransferModel model)
    {
        var res = await _transferService.UpdateAsync(id, model);
        return res ? Ok() : NotFound();
    }

    [Authorize]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var res = await _transferService.DeleteAsync(id);
        return res ? Ok() : NotFound();
    }
    
    [ApiKey]
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Reject(
        [FromHeader(Name = HttpHeaderKeys.RequestId)] [Required] string requestId)
    {
        await _transferService.RejectAsync(requestId);
        return Ok();
    }
}