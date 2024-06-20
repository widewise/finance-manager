using System.IdentityModel.Tokens.Jwt;

namespace FinanceManager.User.Services;

public class AuditLogMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLogMiddleware> _logger;

    public AuditLogMiddleware(RequestDelegate next, ILogger<AuditLogMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        string tokenString = httpContext.Request.Headers["Authorization"]!;

        if (!string.IsNullOrEmpty(tokenString))
        {
            if (tokenString.StartsWith("Bearer "))
            {
                tokenString = tokenString.Substring(7);
            }
            _logger.LogInformation("Token Used: '{Token}' at {Time}", tokenString, DateTime.UtcNow);
            _logger.LogInformation("Request Path: {Path}", httpContext.Request.Path);
            _logger.LogInformation("Request Method: {Method}", httpContext.Request.Method);
            var tokenHandler = new JwtSecurityTokenHandler();
            if (tokenHandler.ReadToken(tokenString) is JwtSecurityToken securityToken)
            {
                var claimsString = string.Join(',', securityToken.Claims.Select(c => $"{c.Type}: {c.Value}"));
                _logger.LogInformation("Token details: Id {Id}; issuer {Issuer}; validFrom: {ValidTo}; validFrom: {ValidTo}; signature algorithm: {SignatureAlgorithm}; claims: {Claims}", 
                    securityToken.Id,
                    securityToken.Issuer,
                    securityToken.ValidFrom,
                    securityToken.ValidTo,
                    securityToken.SignatureAlgorithm,
                    claimsString);
            }
        }

        await _next(httpContext);
    }
}