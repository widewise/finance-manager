using Newtonsoft.Json;

namespace FinanceManager.Testing.Helpers.JwtAuthorisation.Models;

public class Certificates
{
    [JsonProperty("keys")]
    public List<Key> Keys { get; set; }
}
public class Key
{
    [JsonProperty("kid")]
    public string Kid { get; set; }
    [JsonProperty("kty")]
    public string Kty { get; set; }
    [JsonProperty("alg")]
    public string Alg { get; set; }
    [JsonProperty("use")]
    public string Use { get; set; }
    [JsonProperty("n")]
    public string N { get; set; }
    [JsonProperty("e")]
    public string E { get; set; }
    [JsonProperty("x5c")]
    public string[] X5C { get; set; }
    [JsonProperty("x5t")]
    public string X5T { get; set; }
    public string Hash { get; set; }
}