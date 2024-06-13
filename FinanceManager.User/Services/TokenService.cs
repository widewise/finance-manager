using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FinanceManager.Web;
using Microsoft.IdentityModel.Tokens;

namespace FinanceManager.User.Services;

public interface ITokenService
{
    string CreateToken(Models.User user, string[] roles, Claim[] claims);
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

    public string CreateToken(Models.User user, string[] roles, Claim[] claims)
    {
        var expiration = DateTime.UtcNow.AddMinutes(ExpirationMinutes);
        var token = CreateJwtToken(
            CreateClaims(user, roles, claims),
            CreateSigningCredentials(),
            expiration
        );
        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }

    private JwtSecurityToken CreateJwtToken(List<Claim> claims, SigningCredentials credentials,
        DateTime expiration) =>
        new(
            Constants.ValidIssuer,
            Constants.ValidAudience,
            claims,
            expires: expiration,
            signingCredentials: credentials
        );

    private List<Claim> CreateClaims(Models.User user, IList<string> roles, Claim[] otherClaims)
    {
        try
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.Email, user.Email),
                new("AspNet.Identity.SecurityStamp", user.SecurityStamp)
            };
            claims.AddRange(roles.Select(
                role => new Claim(ClaimsIdentity.DefaultRoleClaimType, role)));
            claims.AddRange(otherClaims);
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
        return new SigningCredentials(
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration.GetValue<string>("AppSecurityKey"))
            ),
            SecurityAlgorithms.HmacSha256
        );
    }
}
