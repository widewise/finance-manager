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

[ApiVersion(1.0)]
[Route("api/v{v:apiVersion}/currencies")]
public class CurrencyController : BaseController
{
    private readonly ILogger<CurrencyController> _logger;
    private readonly ICurrencyService _currencyService;
    private readonly IMapper _mapper;
    private readonly ILinkService _linkService;

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
    
    [Authorize]
    [HttpGet(Name = ApiNameConstants.GetCurrency)]
    [ProducesResponseType(typeof(IEnumerable<CurrencyResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
        [FromHeader(Name = HttpHeaderKeys.RequestId)] string? requestId)
    {
        if (!HttpContext.HasUserId(out var currentUserId))
        {
            _logger.LogInformation("The current user identifier is not set");
            return BadRequest("The current user identifier is not set");
        }

        var currencies = await _currencyService.GetAsync(new CurrencyQueryParameters
        {
            RequestId = requestId,
            UserId = currentUserId
        });
        var resp = currencies.Select(GetResponse);
        return Ok(resp);
    }

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

    [Authorize]
    [HttpPost(Name = ApiNameConstants.CreateCurrency)]
    [ProducesResponseType(typeof(CurrencyResponse), StatusCodes.Status201Created)]
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
        if (currency == null)
        {
            return BadRequest();
        }

        var response = GetResponse(currency);
        return Created(response.Links.FirstOrDefault()?.Href ?? string.Empty, response);
    }

    [ApiKey]
    [HttpPost("{userId}/bulk", Name = ApiNameConstants.BulkCreateCurrencies)]
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
    [HttpDelete("{id}", Name = ApiNameConstants.DeleteCurrency)]
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
    [HttpDelete(Name = ApiNameConstants.RejectCurrencies)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Reject(
        [FromHeader(Name = HttpHeaderKeys.RequestId)] [Required] string requestId)
    {
        await _currencyService.RejectAsync(requestId);
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
    
    private CurrencyResponse GetResponse(Currency currency)
    {
        var response = _mapper.Map<CurrencyResponse>(currency);
        var id = response.Id;
        var links = new List<Link>();

        links.Add(_linkService.Generate(
            ApiNameConstants.GetCurrency,
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