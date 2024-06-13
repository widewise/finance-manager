using System.ComponentModel;

namespace FinanceManager.Web;

public enum UserRole
{
    [Description("User")]
    User = 0,
    [Description("Admin")]
    Admin = 1
}