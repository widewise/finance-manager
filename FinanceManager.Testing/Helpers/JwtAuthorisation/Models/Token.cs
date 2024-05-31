using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace FinanceManager.Testing.Helpers.JwtAuthorisation.Models;

public class Token
{
    [JsonProperty("access_token")]
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = null!;
    [JsonProperty("expires_in")]
    [JsonPropertyName("expires_in")]
    public long ExpiresIn { get; set; }
    [JsonProperty("refresh_expires_in")]
    [JsonPropertyName("refresh_expires_in")]
    public long RefreshExpiresIn { get; set; }
    [JsonProperty("refresh_token")]
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; }
    [JsonProperty("token_type")]
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = null!;
    [JsonProperty("not-before-policy")]
    [JsonPropertyName("not-before-policy")]
    public long NotBeforePolicy { get; set; }
    [JsonProperty("session_state")]
    [JsonPropertyName("session_state")]
    public Guid SessionState { get; set; }
    [JsonProperty("scope")]
    [JsonPropertyName("scope")]
    public string Scope { get; set; }
}