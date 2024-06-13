using Microsoft.AspNetCore.Identity;

namespace FinanceManager.User.Models;

public class CustomIdentityRole : IdentityRole<long>
{
    public CustomIdentityRole() { }
    public CustomIdentityRole(string roleName) : base(roleName) { }
}