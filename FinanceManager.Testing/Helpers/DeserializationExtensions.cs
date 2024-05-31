using System.Text.Json;

namespace FinanceManager.Testing.Helpers;

public static class DeserializationExtensions
{
    public static T? ReadAs<T>(this HttpContent content)
    {
        var stringContent = content.ReadAsStringAsync().Result;

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
             
        return JsonSerializer.Deserialize<T>(stringContent, options);
    }    
    
    public static T? ReadAs<T>(this string stringContent)
    {
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
             
        return JsonSerializer.Deserialize<T>(stringContent, options);
    }
}