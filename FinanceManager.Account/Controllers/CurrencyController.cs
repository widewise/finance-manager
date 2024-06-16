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

/// <summary>
/// Controller for <see cref="Currency"/> entity
/// </summary>
[ApiVersion(1.0)]
[ApiVersion(2.0)]
[Route("api/v{v:apiVersion}/currencies")]
[Produces("application/json")]
public class CurrencyController : BaseController
{
    private readonly ILogger<CurrencyController> _logger;
    private readonly ICurrencyService _currencyService;
    private readonly IMapper _mapper;
    private readonly ILinkService _linkService;

    /// <inheritdoc />
    public CurrencyController(
        ILogger<CurrencyController> logger,
        ICurrencyService currencyService,
        IMapper mapper,
        ILinkService linkService)
    {
        _logger = logger;
        _currencyService = currencyService;
        _mapper = mapper;
        _linkService = linkService;
    }

    /// <summary>
    /// Fetch <see cref="Currency"/> collection
    /// </summary>
    /// <param name="requestId">Request identifier</param>
    /// <returns>The <see cref="Currency"/> collection</returns>
    /// <response code="200">Returns currencies collection</response>
    /// <response code="400">Returns if user identifier in token isn't set</response>
    [Authorize]
    [MapToApiVersion(1.0)]
    [HttpGet(Name = ApiNameConstants.GetCurrencyV1)]
    [ProducesResponseType(typeof(IEnumerable<Currency>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetV1(
        [FromHeader(Name = HttpHeaderKeys.RequestId)] string? requestId)
    {
        if (!HttpContext.HasUserId(out var currentUserId))
        {
            _logger.LogInformation(MessageConstants.CurrentUserMessageIsNotSetMessage);
            return BadRequest(MessageConstants.CurrentUserMessageIsNotSetMessage);
        }

        return Ok(await _currencyService.GetAsync(new CurrencyQueryParameters
        {
            RequestId = requestId,
            UserId = currentUserId
        }));
    }

    /// <summary>
    /// Fetch <see cref="CurrencyResponse"/> collection
    /// </summary>
    /// <param name="requestId">Request identifier</param>
    /// <returns>The <see cref="CurrencyResponse"/> collection</returns>
    /// <response code="200">Returns currencies collection</response>
    /// <response code="400">Returns if user identifier in token isn't set</response>
    [Authorize]
    [MapToApiVersion(2.0)]
    [HttpGet(Name = ApiNameConstants.GetCurrencyV2)]
    [ProducesResponseType(typeof(IEnumerable<CurrencyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetV2(
        [FromHeader(Name = HttpHeaderKeys.RequestId)] string? requestId)
    {
        if (!HttpContext.HasUserId(out var currentUserId))
        {
            _logger.LogInformation(MessageConstants.CurrentUserMessageIsNotSetMessage);
            return BadRequest(MessageConstants.CurrentUserMessageIsNotSetMessage);
        }

        var currencies = await _currencyService.GetAsync(new CurrencyQueryParameters
        {
            RequestId = requestId,
            UserId = currentUserId
        });
        var resp = currencies.Select(GetResponse);
        return Ok(resp);
    }

    /// <summary>
    /// Create <see cref="Currency"/> from <see cref="CreateCurrencyModel"/> parameters model
    /// </summary>
    /// <param name="requestId">Request identifier</param>
    /// <param name="model">The <see cref="CreateCurrencyModel"/> parameters model</param>
    /// <returns>The <see cref="Currency"/> model</returns>
    /// <response code="200">Returns <see cref="Currency"/> model</response>
    /// <response code="400">Returns if user identifier in token isn't set</response>
    [Authorize]
    [MapToApiVersion(1.0)]
    [HttpPost(Name = ApiNameConstants.CreateCurrencyV1)]
    [ProducesResponseType(typeof(CurrencyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateV1(
        [FromHeader(Name = HttpHeaderKeys.RequestId)][Required] string requestId,
        [FromBody] CreateCurrencyModel model)
    {
        if (!HttpContext.HasUserId(out var currentUserId))
        {
            _logger.LogInformation(MessageConstants.CurrentUserMessageIsNotSetMessage);
            return BadRequest(MessageConstants.CurrentUserMessageIsNotSetMessage);
        }

        var currency = await _currencyService.CreateAsync(requestId, currentUserId, model);
        if (currency == null)
        {
            return BadRequest();
        }

        return Ok(currency);
    }

    /// <summary>
    /// Create <see cref="Currency"/> from <see cref="CreateCurrencyModel"/> parameters model
    /// </summary>
    /// <param name="requestId">Request identifier</param>
    /// <param name="model">The <see cref="CreateCurrencyModel"/> parameters model</param>
    /// <returns>The <see cref="CurrencyResponse"/> model</returns>
    /// <response code="201">Returns <see cref="CurrencyResponse"/> model</response>
    /// <response code="400">Returns if user identifier in token isn't set</response>
    [Authorize]
    [MapToApiVersion(2.0)]
    [HttpPost(Name = ApiNameConstants.CreateCurrencyV2)]
    [ProducesResponseType(typeof(CurrencyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateV2(
        [FromHeader(Name = HttpHeaderKeys.RequestId)][Required] string requestId,
        [FromBody] CreateCurrencyModel model)
    {
        if (!HttpContext.HasUserId(out var currentUserId))
        {
            _logger.LogInformation(MessageConstants.CurrentUserMessageIsNotSetMessage);
            return BadRequest(MessageConstants.CurrentUserMessageIsNotSetMessage);
        }

        var currency = await _currencyService.CreateAsync(requestId, currentUserId, model);
        if (currency == null)
        {
            return BadRequest();
        }

        var response = GetResponse(currency);
        return Created(response.Links.FirstOrDefault()?.Href ?? string.Empty, response);
    }

    /// <summary>
    /// Fetch <see cref="Currency"/> collection for internal purpose (other services)
    /// </summary>
    /// <param name="requestId">Request identifier</param>
    /// <param name="userId">User identifier</param>
    /// <returns>The <see cref="Currency"/> collection</returns>
    /// <response code="200">Returns currencies collection</response>
    [ApiKey]
    [HttpGet("internal", Name = ApiNameConstants.GetInternalCurrency)]
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

    /// <summary>
    /// Create multiple <see cref="Currency"/> entities from <see cref="CreateCurrencyModel"/> collection. Used for internal purpose (other services)
    /// </summary>
    /// <param name="requestId">Request identifier</param>
    /// <param name="userId">User identifier</param>
    /// <param name="models">The <see cref="CreateCurrencyModel"/> collection</param>
    /// <returns>The <see cref="Currency"/> collection</returns>
    /// <response code="200">Returns <see cref="Currency"/> collection</response>
    /// <response code="400">Returns if some problems during operation happen</response>
    [ApiKey]
    [HttpPost("{userId}/bulk", Name = ApiNameConstants.BulkCreateCurrencies)]
    [ProducesResponseType(typeof(IEnumerable<Currency>),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BulkCreate(
        [FromHeader(Name = HttpHeaderKeys.RequestId)][Required] string requestId,
        [FromRoute] long userId,
        [FromBody] CreateCurrencyModel[] models)
    {
        var currencies = await _currencyService.BulkCreateAsync(models, userId, requestId);
        return currencies != null ? Ok(currencies) : BadRequest();
    }

    /// <summary>
    /// Delete <see cref="Currency"/>  by identifier
    /// </summary>
    /// <param name="id">The <see cref="Currency"/> identifier</param>
    /// <response code="204">If <see cref="Currency"/> with identifier from parameters is deleted</response>
    /// <response code="400">Returns if some problems during operation happen</response>
    [Authorize]
    [HttpDelete("{id}", Name = ApiNameConstants.DeleteCurrency)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        if (!HttpContext.HasUserId(out var currentUserId))
        {
            _logger.LogInformation(MessageConstants.CurrentUserMessageIsNotSetMessage);
            return BadRequest(MessageConstants.CurrentUserMessageIsNotSetMessage);
        }

        var res = await _currencyService.DeleteAsync(id, currentUserId);
        return res ? NoContent() : BadRequest();
    }

    /// <summary>
    /// Reject changes by request identifier
    /// </summary>
    /// <param name="requestId">Request identifier</param>
    /// <response code="204">If changes for request identifier are rejected</response>
    [ApiKey]
    [HttpDelete(Name = ApiNameConstants.RejectCurrencies)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Reject(
        [FromHeader(Name = HttpHeaderKeys.RequestId)] [Required] string requestId)
    {
        await _currencyService.RejectAsync(requestId);
        return NoContent();
    }

    /// <summary>
    /// Get meta documentation
    /// </summary>
    /// <response code="200">Returns meta information</response>
    [AllowAnonymous]
    [HttpOptions]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult DocumentsOptions()
    {
        AddAllowHeader();
        return Ok();
    }

    private CurrencyResponse GetResponse(Currency currency)
    {
        var response = _mapper.Map<CurrencyResponse>(currency);
        var id = response.Id;
        var links = new List<Link>();

        links.Add(_linkService.Generate(
            ApiNameConstants.GetCurrencyV2,
            new (),
            ApiNameConstants.Self,
            HttpMethod.Get.Method));

        links.Add(_linkService.Generate(
            ApiNameConstants.DeleteCurrency,
            new { id },
            ApiNameConstants.DeleteCurrencyRel,
            HttpMethod.Delete.Method));

        response.Links = links.ToArray();
        return response;
    }
}