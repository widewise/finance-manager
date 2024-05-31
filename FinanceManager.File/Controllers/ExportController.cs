using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using FinanceManager.File.Services;
using FinanceManager.Web;
using FinanceManager.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.File.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{v:apiVersion}/exports")]
public class ExportController : ControllerBase
{
    private readonly ILogger<ExportController> _logger;
    private readonly IExportSessionService _service;

    public ExportController(
        ILogger<ExportController> logger,
        IExportSessionService service)
    {
        _logger = logger;
        _service = service;
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(
        [FromHeader(Name = HttpHeaderKeys.RequestId)] [Required]
        string requestId)
    {
        if (!HttpContext.HasUserId(out var currentUserId))
        {
            _logger.LogInformation("The current user email is not set");
            return BadRequest("The current user email is not set");
        }

        var id = await _service.CreateAsync(currentUserId, requestId);
        if (id == null)
        {
            return BadRequest();
        }

        return Ok(id);
    }
    
    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Download([FromRoute] long id)
    {
        var stream = await _service.DownloadAsync(id);
        if (stream == null)
        {
            return BadRequest();
        }

        return File(
            stream,
            "application/octet-stream",
            $"FinanceManager-{DateTime.UtcNow.ToShortDateString()}.json");
    }
    
    [Authorize]
    [HttpGet("{id}/state")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetState([FromRoute] long id)
    {
        var state = await _service.GetStateAsync(id);
        if (!state.HasValue)
        {
            return BadRequest();
        }

        return Ok(state.Value);
    }
}