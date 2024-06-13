using FinanceManager.Web;

namespace FinanceManager.User.Models;

public class RegistrationModel
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public UserRole Role { get; set; }
}