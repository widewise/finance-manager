using Asp.Versioning;
using FinanceManager.User.Models;
using FinanceManager.User.Services;
using FinanceManager.Web.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.User.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{v:apiVersion}/[controller]")]
public class TokenController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ITokenService _tokenService;
    private readonly IUserRoleService _userRoleService;

    public TokenController(
        AppDbContext dbContext,
        ITokenService tokenService,
        IUserRoleService userRoleService)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
        _userRoleService = userRoleService;
    }

    [HttpPost]
    [Route("refresh")]
    public async Task<IActionResult> Refresh(TokenRefreshRequestModel tokenRefreshRequestModel)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(tokenRefreshRequestModel.AccessToken);
        if (principal.Identity == null)
        {
            return BadRequest("Wrong identity principal value");
        }
        var username = principal.Identity.Name;
        var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.UserName == username);
        if (user is null ||
            user.RefreshToken != tokenRefreshRequestModel.RefreshToken ||
            user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return BadRequest("Invalid client request");
        }

        var (userRoles, userClaims) = await _userRoleService.GetRolesAndClaimsAsync(user);

        var newAccessToken = _tokenService.CreateToken(user, userRoles.ToArray(), userClaims.ToArray());
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(TokenConstants.RefreshTokenExpirationHours);

        await _dbContext.SaveChangesAsync();
        return Ok(new TokenRefreshResponseModel
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        });
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost]
    [Route("revoke")]
    public async Task<IActionResult> Revoke()
    {
        if (!HttpContext.HasUserId(out var userId))
        {
            return BadRequest("The current user identifier is not set");
        }

        var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return BadRequest("Invalid client request");
        }

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }
}