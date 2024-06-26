using Asp.Versioning;
using FinanceManager.User.Models;
using FinanceManager.User.Services;
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
        if (!_tokenService.ValidateRefreshToken(tokenRefreshRequestModel.RefreshToken))
        {
            return BadRequest("Invalid client request");
        }

        var principal = _tokenService.GetPrincipalFromExpiredToken(tokenRefreshRequestModel.AccessToken);
        if (principal.Identity == null)
        {
            return BadRequest("Wrong identity principal value");
        }

        var username = principal.Identity.Name;
        var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.UserName == username);
        if (user is null)
        {
            return BadRequest("Invalid client request");
        }

        var (userRoles, userClaims) = await _userRoleService.GetRolesAndClaimsAsync(user);

        var newAccessToken = _tokenService.CreateToken(user, userRoles.ToArray(), userClaims.ToArray());
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        await _dbContext.SaveChangesAsync();
        return Ok(new TokenRefreshResponseModel
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        });
    }
}