using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FinanceManager.Web;
using Microsoft.IdentityModel.Tokens;

namespace FinanceManager.User.Services;

public interface ITokenService
{
    string CreateToken(Models.User user);
}

public class TokenService: ITokenService
{
    private readonly ILogger<TokenService> _logger;
    private readonly IConfiguration _configuration;
    private const int ExpirationMinutes = 30;

    public TokenService(
        ILogger<TokenService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public string CreateToken(Models.User user)
    {
        var expiration = DateTime.UtcNow.AddMinutes(ExpirationMinutes);
        var token = CreateJwtToken(
            CreateClaims(user),
            CreateSigningCredentials(),
            expiration
        );
        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }

    private static JwtSecurityToken CreateJwtToken(List<Claim> claims, SigningCredentials credentials,
        DateTime expiration) =>
        new(
            Constants.ValidIssuer,
            Constants.ValidAudience,
            claims,
            expires: expiration,
            signingCredentials: credentials
        );

    private List<Claim> CreateClaims(Models.User user)
    {
        try
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.UserName ?? string.Empty),
                new(ClaimTypes.Email, user.Email ?? string.Empty)
            };
            return claims;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Create claims error: {ErrorMessage}", e.Message);
            throw;
        }
    }

    private SigningCredentials CreateSigningCredentials()
    {
        var appSecurityKey = _configuration.GetValue<string>("AppSecurityKey");
        if (appSecurityKey == null)
        {
            throw new ArgumentNullException(nameof(appSecurityKey));
        }

        return new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSecurityKey)),
            SecurityAlgorithms.HmacSha256);
    }
}
