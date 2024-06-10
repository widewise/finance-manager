using System.Security.Claims;
using FinanceManager.User.Models;
using FinanceManager.Web;
using FinanceManager.Web.Extensions;
using Microsoft.AspNetCore.Identity;

namespace FinanceManager.User.Migrations;

public interface IDbInitializer
{
    Task InitializeAsync();
}

public class DbInitializer : IDbInitializer
{
    private readonly ILogger<DbInitializer> _logger;
    private readonly RoleManager<CustomIdentityRole> _roleManager;

    public DbInitializer(
        ILogger<DbInitializer> logger,
        RoleManager<CustomIdentityRole> roleManager)
    {
        _logger = logger;
        _roleManager = roleManager;
    }

    public async Task InitializeAsync()
    {
        await CreateRoleAsync(UserRole.Admin.DisplayName(),
            AuthClaims.CategoryRead, AuthClaims.CategoryWrite,
            AuthClaims.CurrencyRead, AuthClaims.CurrencyWrite);
        await CreateRoleAsync(UserRole.User.DisplayName(),
            AuthClaims.CategoryRead,
            AuthClaims.CurrencyRead);
    }

    private async Task CreateRoleAsync(string roleName, params string[] roleClaims)
    {
        var roleExist = await _roleManager.RoleExistsAsync(roleName);
        if (roleExist)
        {
            return;
        }

        var role = new CustomIdentityRole(roleName);
        var createRoleResult = await _roleManager.CreateAsync(role);
        if (!createRoleResult.Succeeded)
        {
            _logger.LogError(string.Join("\n",
                createRoleResult.Errors.Select(e => $"{e.Code}: {e.Description}")));
            return;
        }

        foreach (var roleClaim in roleClaims)
        {
            var addRoleClaimResult = await _roleManager.AddClaimAsync(
                role,
                new Claim(AuthClaimTypes.Permission, roleClaim));
            if (addRoleClaimResult.Succeeded) continue;

            _logger.LogError(string.Join("\n",
                createRoleResult.Errors.Select(e => $"{e.Code}: {e.Description}")));
            return;
        }
    }
}