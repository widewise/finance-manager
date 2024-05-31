using FinanceManager.Testing.Helpers.JwtAuthorisation.Models;

namespace FinanceManager.Testing.Helpers.JwtAuthorisation;

public static class AuthResponseGenerator
{
    public static OpenidConfiguration GetOpenidConfiguration(string baseAuthUrl)
    {
        return new OpenidConfiguration
        {
            Issuer = baseAuthUrl,
            AuthorizationEndpoint = $"{baseAuthUrl}/connect/authorize",
            TokenEndpoint = $"{baseAuthUrl}/connect/token",
            UserinfoEndpoint = $"{baseAuthUrl}/connect/userinfo",
            EndSessionEndpoint = $"{baseAuthUrl}/connect/endsession",
            JwksUri = $"{baseAuthUrl}/.well-known/openid-configuration/jwks",
            CheckSessionIframe = $"{baseAuthUrl}/connect/checksession",
            IntrospectionEndpoint = $"{baseAuthUrl}/connect/introspect",
            RevocationEndpoint = $"{baseAuthUrl}/connect/revocation",
            DeviceAuthorizationEndpoint = $"{baseAuthUrl}/connect/deviceauthorization",
            RequestParameterSupported = true,
            FrontchannelLogoutSupported = true,
            FrontchannelLogoutSessionSupported = true,
            BackchannelLogoutSupported = true,
            BackchannelLogoutSessionSupported = true,
            ScopesSupported = new List<string>
            {
                "openid",
                "profile",
                "email",
                "role",
                "api1.read",
                "api1.write",
                "offline_access"
            },
            ClaimsSupported = new List<string>
            {
                "sub",
                "name",
                "family_name",
                "given_name",
                "middle_name",
                "nickname",
                "preferred_username",
                "profile",
                "picture",
                "website",
                "gender",
                "birthdate",
                "zoneinfo",
                "locale",
                "updated_at",
                "email",
                "email_verified",
                "role"
            },
            GrantTypesSupported = new List<string>
            {
                "authorization_code",
                "client_credentials",
                "refresh_token",
                "implicit",
                "password",
                "urn:ietf:params:oauth:grant-type:device_code"
            },
            ResponseTypesSupported = new List<string>
            {
                "code",
                "token",
                "id_token",
                "id_token token",
                "code id_token",
                "code token",
                "code id_token token"
            },
            ResponseModesSupported = new List<string>
            {
                "query",
                "fragment",
                "form_post"
            },
            TokenEndpointAuthMethodsSupported = new List<string>
            {
                "client_secret_basic",
                "client_secret_post"
            },
            IdTokenSigningAlgValuesSupported = new List<string>
            {
                "RS256"
            },
            SubjectTypesSupported = new List<string>
            {
                "public"
            },
            CodeChallengeMethodsSupported = new List<string>
            {
                "plain",
                "S256"
            }
        };
    }

    public static Certificates GetCertificates()
    {
        return new Certificates
        {
            Keys = new List<Key>
            {
                new()
                {
                    Kty = "RSA",
                    Use = "sig",
                    Kid = "182A1C8AF84CA98E93E689653F8458FF",
                    E = "AQAB",
                    N = "0Q3XWsxqGkwDtHcMv-zyCb-cN-4u-n1eIVZkx1nzUO8m3hAAvP1q5yn1ZTzcehUnm3z5GXJoAJiIie2n-OrwkOW47V74KAvUuyO1uMadZ-6V3th0OlwhE84uOpm5RnTl1VJRTzgfqey9o7GcJcpq9NeugCE0Irobbf6Nee1Cs3wKGtRKDAf98Mn7RDdn00E3B958GSISsIJKvhXawEAPk9STDhodf-rhZ7T2CNt9aNsOQT07Ia4MFci7vYMwM0JnmVb7ItSQk3GhpvDq7v4ufWncWKWlEdUuzdRCkKGozQ3ajmqNxHZoDCdCEATsS-4a0zTzQv0eUBdAz7IlEk2k2Q",
                    Alg = "RS256"
                }
            }
        };
    }

    public static Token GetToken(string scope, string securityKey, AuthorizationUser user, Dictionary<string, string>? claims = null)
    {
        return new Token
        {
            AccessToken = new JwtTokenBuilder(securityKey).WithUser(user).WithClaims(claims).CreateToken(),
            ExpiresIn = 100000,
            NotBeforePolicy = 1518534968,
            SessionState = Guid.NewGuid(),
            Scope = scope,
            RefreshToken = new JwtTokenBuilder(securityKey).CreateToken(),
            RefreshExpiresIn = 18000000,
            TokenType = "Bearer"
        };
    }
}