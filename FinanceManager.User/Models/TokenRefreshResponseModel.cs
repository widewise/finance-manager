namespace FinanceManager.User.Models;

public class TokenRefreshResponseModel
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}