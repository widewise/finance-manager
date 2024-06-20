namespace FinanceManager.User.Models;

public class AuthResponseModel
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string Token { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}