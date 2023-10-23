namespace FinanceManager.User.Models;

public class AuthRequestModel
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}