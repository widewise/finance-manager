using Newtonsoft.Json;

namespace FinanceManager.Testing.Helpers.JwtAuthorisation.Models;

public class OpenidConfiguration
{
    [JsonProperty("issuer")]
    public string Issuer { get; set; } = null!;
    [JsonProperty("authorization_endpoint")]
    public string AuthorizationEndpoint { get; set; }  = null!;
    [JsonProperty("token_endpoint")]
    public string? TokenEndpoint { get; set; }
    [JsonProperty("userinfo_endpoint")]
    public string? UserinfoEndpoint { get; set; }
    [JsonProperty("end_session_endpoint")]
    public string? EndSessionEndpoint { get; set; }
    [JsonProperty("jwks_uri")]
    public string JwksUri { get; set; } = null!;
    [JsonProperty("check_session_iframe")]
    public string CheckSessionIframe { get; set; } = null!;
    [JsonProperty("revocation_endpoint")]
    public string RevocationEndpoint { get; set; } = null!;
    [JsonProperty("device_authorization_endpoint")]
    public string DeviceAuthorizationEndpoint { get; set; } = null!;
    [JsonProperty("grant_types_supported")]
    public List<string>? GrantTypesSupported { get; set; }
    [JsonProperty("response_types_supported")]
    public List<string> ResponseTypesSupported { get; set; } = null!;
    [JsonProperty("subject_types_supported")]
    public List<string> SubjectTypesSupported { get; set; } = null!;
    [JsonProperty("id_token_signing_alg_values_supported")]
    public List<string> IdTokenSigningAlgValuesSupported { get; set; } = null!;
    [JsonProperty("response_modes_supported")]
    public List<string>? ResponseModesSupported { get; set; }
    [JsonProperty("token_endpoint_auth_methods_supported")]
    public List<string>? TokenEndpointAuthMethodsSupported { get; set; }
    [JsonProperty("claims_supported")]
    public List<string> ClaimsSupported { get; set; }
    [JsonProperty("claims_parameter_supported")]
    public bool ClaimsParameterSupported { get; set; }
    [JsonProperty("scopes_supported")]
    public List<string>? ScopesSupported { get; set; }
    [JsonProperty("request_parameter_supported")]
    public bool RequestParameterSupported { get; set; }
    [JsonProperty("request_uri_parameter_supported")]
    public bool RequestUriParameterSupported { get; set; }
    [JsonProperty("code_challenge_methods_supported")]
    public List<string>? CodeChallengeMethodsSupported { get; set; }
    [JsonProperty("tls_client_certificate_bound_access_tokens")]
    public bool TlsClientCertificateBoundAccessTokens { get; set; }
    [JsonProperty("frontchannel_logout_supported")]
    public bool FrontchannelLogoutSupported { get; set; }
    [JsonProperty("frontchannel_logout_session_supported")]
    public bool FrontchannelLogoutSessionSupported { get; set; }
    [JsonProperty("backchannel_logout_supported")]
    public bool BackchannelLogoutSupported { get; set; }
    [JsonProperty("backchannel_logout_session_supported")]
    public bool BackchannelLogoutSessionSupported { get; set; }
    
    [JsonProperty("introspection_endpoint")]
    public string IntrospectionEndpoint { get; set; }
}