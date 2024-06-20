using System.Security.Claims;
using FinanceManager.User.Models;
using Microsoft.AspNetCore.Identity;

namespace FinanceManager.User.Services;

public interface IUserRoleService
{
    Task<(IEnumerable<string> userRoles, IEnumerable<Claim> userClaims)> GetRolesAndClaimsAsync(Models.User user);
}

public class UserRoleService : IUserRoleService
{
    private readonly UserManager<Models.User> _userManager;
    private readonly RoleManager<CustomIdentityRole> _roleManager;

    public UserRoleService(
        UserManager<Models.User> userManager,
        RoleManager<CustomIdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }
    public async Task<(IEnumerable<string>, IEnumerable<Claim>)> GetRolesAndClaimsAsync(Models.User user)
    {
        var userRoles = await _userManager.GetRolesAsync(user);
        var userClaims = new List<Claim>();
        foreach (var userRole in userRoles)
        {
            var role = await _roleManager.FindByNameAsync(userRole);
            if (role == null)
            {
                continue;
            }
            userClaims.AddRange(await _roleManager.GetClaimsAsync(role));
        }

        return (userRoles, userClaims);
    }
}