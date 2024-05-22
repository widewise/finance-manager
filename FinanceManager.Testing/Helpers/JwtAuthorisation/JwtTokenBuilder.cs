using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FinanceManager.Testing.Helpers.JwtAuthorisation.Models;
using FinanceManager.Web;
using Microsoft.IdentityModel.Tokens;

namespace FinanceManager.Testing.Helpers.JwtAuthorisation;

public class JwtTokenBuilder
{
    private const int ExpirationMinutes = 30;
    private readonly List<Claim> _claims = new();
    private readonly string _securityKey;

    public JwtTokenBuilder(string securityKey)
    {
        _securityKey = securityKey;
    }

    public string CreateToken()
    {
        var expiration = DateTime.UtcNow.AddMinutes(ExpirationMinutes);
        var token = CreateJwtToken(
            _claims,
            CreateSigningCredentials(),
            expiration
        );
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenString = tokenHandler.WriteToken(token);
        return tokenString;
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

    private SigningCredentials CreateSigningCredentials() =>
        new(
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_securityKey)
            ),
            SecurityAlgorithms.HmacSha256
        );

    public JwtTokenBuilder WithClaims(Dictionary<string, string>? withClaims)
    {
        if (withClaims != null)
        {
            foreach (var claim in withClaims)
            {
                _claims.Add(new Claim(claim.Key, claim.Value));
            }
        }

        return this;
    }

    public JwtTokenBuilder WithUser(AuthorizationUser user)
    {
        _claims.AddRange(new Claim[]
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email)
        });
        return this;
    }
}