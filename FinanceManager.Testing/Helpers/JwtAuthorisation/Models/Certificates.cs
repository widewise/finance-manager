using Newtonsoft.Json;

namespace FinanceManager.Testing.Helpers.JwtAuthorisation.Models;

public class Certificates
{
    [JsonProperty("keys")]
    public List<Key> Keys { get; set; } = null!;
}
public class Key
{
    [JsonProperty("kid")]
    public string Kid { get; set; } = null!;

    [JsonProperty("kty")]
    public string Kty { get; set; } = null!;
    [JsonProperty("alg")]
    public string Alg { get; set; } = null!;
    [JsonProperty("use")]
    public string Use { get; set; } = null!;
    [JsonProperty("n")]
    public string N { get; set; } = null!;
    [JsonProperty("e")]
    public string E { get; set; } = null!;
    [JsonProperty("x5c")]
    public string[] X5C { get; set; } = null!;
    [JsonProperty("x5t")]
    public string X5T { get; set; } = null!;
}