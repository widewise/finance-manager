using System.ComponentModel.DataAnnotations;
using FinanceManager.File.Services;
using FinanceManager.TransportLibrary;
using FinanceManager.TransportLibrary.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.File.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImportController : ControllerBase
{
    private readonly ILogger<ImportController> _logger;
    private readonly IImportSessionService _service;

    public ImportController(
        ILogger<ImportController> logger,
        IImportSessionService service)
    {
        _logger = logger;
        _service = service;
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(
        [FromHeader(Name = HttpHeaderKeys.RequestId)] [Required] string requestId,
        IFormFile formFile)
    {
        if (!HttpContext.HasUserId(out var currentUserId))
        {
            _logger.LogInformation("The current user email is not set");
            return BadRequest("The current user email is not set");
        }
        var id = await _service.CreateAsync(
            requestId,
            currentUserId,
            formFile.FileName,
            formFile.OpenReadStream());
        if (id == null)
        {
            return BadRequest();
        }

        return Ok(id);
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