using Asp.Versioning;
using FinanceManager.User.Models;
using FinanceManager.User.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.User.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{v:apiVersion}/auth")]
public class ApiAuthController : ControllerBase
{
    private readonly ILogger<ApiAuthController> _logger;
    private readonly UserManager<Models.User> _userManager;
    private readonly ITokenService _tokenService;
    private readonly AppDbContext _context;

    public ApiAuthController(
        ILogger<ApiAuthController> logger,
        UserManager<Models.User> userManager,
        ITokenService tokenService,
        AppDbContext context)
    {
        _logger = logger;
        _userManager = userManager;
        _tokenService = tokenService;
        _context = context;
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register(RegistrationModel request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _userManager.CreateAsync(
            new Models.User { UserName = request.Username, Email = request.Email },
            request.Password
        );
        if (result.Succeeded)
        {
            request.Password = string.Empty;
            return CreatedAtAction(nameof(Register), new {email = request.Email}, request);
        }

        foreach (var error in result.Errors)
        {
            _logger.LogError(
                "User registration error {ErrorCode}: {ErrorDescription}",
                error.Code,
                error.Description);
            ModelState.AddModelError(error.Code, error.Description);
        }
        return BadRequest(ModelState);
    }

    [HttpPost]
    [Route("login")]
    public async Task<ActionResult<AuthResponseModel>> Authenticate([FromBody] AuthRequestModel request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var managedUser = await _userManager.FindByEmailAsync(request.Email);
        if (managedUser == null)
        {
            return BadRequest("Bad credentials");
        }
        var isPasswordValid = await _userManager.CheckPasswordAsync(managedUser, request.Password);
        if (!isPasswordValid)
        {
            return BadRequest("Bad credentials");
        }
        var userInDb = _context.Users.FirstOrDefault(u => u.Email == request.Email);
        if (userInDb is null)
        {
            return Unauthorized();
        }

        var accessToken = _tokenService.CreateToken(userInDb);
        await _context.SaveChangesAsync();
        return Ok(new AuthResponseModel
        {
            Username = userInDb.UserName,
            Email = userInDb.Email,
            Token = accessToken
        });
    }
}