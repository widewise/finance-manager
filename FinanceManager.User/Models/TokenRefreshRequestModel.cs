namespace FinanceManager.User.Models;

public class TokenRefreshRequestModel
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}