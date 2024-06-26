using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FinanceManager.Web;
using Microsoft.IdentityModel.Tokens;

namespace FinanceManager.User.Services;

public interface ITokenService
{
    string CreateToken(Models.User user, string[] roles, Claim[] claims);
    string GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    bool ValidateRefreshToken(string refreshToken);
}

public class TokenService : ITokenService
{
    private readonly ILogger<TokenService> _logger;
    private readonly IConfiguration _configuration;

    public TokenService(
        ILogger<TokenService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public string CreateToken(Models.User user, string[] roles, Claim[] claims)
    {
        var expiration = DateTime.UtcNow.AddMinutes(TokenConstants.AccessTokenExpirationMinutes);
        var token = CreateJwtToken(
            CreateClaims(user, roles, claims),
            CreateSigningCredentials(_configuration.GetValue<string>("AppSecurityKey")!),
            expiration
        );
        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var expiration = DateTime.UtcNow.AddHours(TokenConstants.RefreshTokenExpirationHours);
        var token = new JwtSecurityToken(
            expires: expiration,
            signingCredentials: CreateSigningCredentials(_configuration.GetValue<string>("AppRefreshSecurityKey")!));
        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = GetSymmetricSecurityKey(_configuration.GetValue<string>("AppSecurityKey")!),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
        var jwtSecurityToken = securityToken as JwtSecurityToken;
        if (jwtSecurityToken == null ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }
            
        return principal;
    }

    public bool ValidateRefreshToken(string refreshToken)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = GetSymmetricSecurityKey(_configuration.GetValue<string>("AppRefreshSecurityKey")!),
            ValidateLifetime = true
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(refreshToken, tokenValidationParameters, out var securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (principal == null || jwtSecurityToken == null ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Refresh token validation error: {ErrorMessage}", e.Message);
            return false;
        }
            
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

    private SigningCredentials CreateSigningCredentials(string securityKey)
    {
        return new SigningCredentials(
            GetSymmetricSecurityKey(securityKey),
            SecurityAlgorithms.HmacSha256);
    }

    private SymmetricSecurityKey GetSymmetricSecurityKey(string securityKey)
    {
        return new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(securityKey)
        );
    }
}