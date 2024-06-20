using Asp.Versioning;
using FinanceManager.User.Models;
using FinanceManager.Web.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.User.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[ApiVersion(1.0)]
[Route("api/v{v:apiVersion}/users")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly UserManager<Models.User> _userManager;
    private readonly SignInManager<Models.User> _signInManager;

    public UserController(
        ILogger<UserController> logger,
        UserManager<Models.User> userManager,
        SignInManager<Models.User> signInManager)
    {
        _logger = logger;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(Models.User), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get([FromRoute] long userId)
    {
        if (!HttpContext.HasUserId(out var currentUserId))
        {
            _logger.LogInformation("The current user identifier is not set");
            return BadRequest("The current user identifier is not set");
        }

        if (userId != currentUserId)
        {
            _logger.LogInformation(
                "Can't read another user data: Current user id {CurrentUserId} and request user id {UserId}",
                currentUserId,
                userId);
            return BadRequest("Can't read another user data");
        }

        var user = await _userManager.FindByIdAsync(currentUserId.ToString());
        if (user == null)
        {
            _logger.LogInformation("User with id {UserId} is not found", currentUserId);
            return NotFound(new Error("404", $"User is not found"));
        }

        if (string.IsNullOrEmpty(user.Email))
        {
            _logger.LogInformation("Empty user email");
            return BadRequest("Empty user email");
        }

        return Ok(new UserModel
        {
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone
        });
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CreateUser([FromBody] UserModel createUser)
    {
        var result = await _userManager.CreateAsync(
            new Models.User
            {
                UserName = createUser.Username,
                Email = createUser.Email,
                Phone = createUser.Phone,
                FirstName = createUser.FirstName,
                LastName = createUser.LastName
            });

        if (result.Succeeded) return NoContent();

        var error = string.Join(";", result.Errors.Select(x => $"{x.Code}:{x.Description}"));
        _logger.LogError("Create user error: {Error}", error);
        return BadRequest($"Create user error: {error}");
    }

    [HttpPut("{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser([FromBody] UserModel user)
    {
        if (!HttpContext.HasUserId(out var currentUserId))
        {
            _logger.LogInformation("The current user identifier is not set");
            return BadRequest("The current user identifier is not set");
        }

        var existedUser = await _userManager.FindByIdAsync(currentUserId.ToString());
        if (existedUser == null)
        {
            _logger.LogInformation("User with id {UserId} is not found", currentUserId);
            return NotFound(new Error("404", $"User is not found"));
        }

        existedUser.Username = user.Username;
        existedUser.FirstName = user.FirstName;
        existedUser.LastName = user.LastName;
        existedUser.Email = user.Email;
        existedUser.Phone = user.Phone;
        var result = await _userManager.UpdateAsync(existedUser);
        if (!result.Succeeded)
        {
            var error = string.Join(";", result.Errors.Select(x => $"{x.Code}:{x.Description}"));
            _logger.LogError("Update user error: {Error}", error);
            return BadRequest($"Update user error: {error}");
        }

        await _userManager.UpdateSecurityStampAsync(existedUser);
        await _signInManager.RefreshSignInAsync(existedUser);

        return NoContent();
    }

    [HttpDelete("{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser([FromRoute] long userId)
    {
        var existedUser = await _userManager.FindByIdAsync(userId.ToString());
        if (existedUser == null)
        {
            _logger.LogInformation("User with id {UserId} is not found", userId);
            return NotFound(new Error("404", $"User with id {userId} is not found"));
        }

        var result = await _userManager.DeleteAsync(existedUser);
        if (!result.Succeeded)
        {
            var error = string.Join(";", result.Errors.Select(x => $"{x.Code}:{x.Description}"));
            _logger.LogError("Delete user error: {Error}", error);
            return BadRequest($"Delete user error: {error}");
        }

        await _signInManager.RefreshSignInAsync(existedUser);

        return NoContent();
    }
}