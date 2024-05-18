using System.Net;

namespace FinanceManager.Testing.Helpers.Clients.RabbitMq;

public class RabbitMqClient
{
    private readonly string _rabbitMqHost;
    private readonly HttpClient _httpClient;
    const string Vhost = "%2F";

    public RabbitMqClient(string rabbitMqHost, string rabbitMqUser, string rabbitMqPassword)
    {
        _rabbitMqHost = rabbitMqHost;
        var httpClientHandler = new HttpClientHandler
        {
            Credentials = new NetworkCredential(rabbitMqUser, rabbitMqPassword)
        };
        _httpClient = new HttpClient(httpClientHandler);
    }

    public async Task<ModelMessageRabbit?> GetMessageFromQueue(string queue)
    {
        var content =
            new StringContent(
                "{\"count\":1,\"ackmode\":\"ack_requeue_false\",\"encoding\":\"auto\",\"truncate\":50000}");
        var httpResponseMessage =
            await _httpClient.PostAsync($"http://localhost:15672/api/queues/{Vhost}/{queue}/get", content);

        var payload = httpResponseMessage.Content.ReadAs<ModelMessageRabbit[]>();
        return payload!.FirstOrDefault();
    }

    public async Task ClearQueue(string queue) =>
        await _httpClient.DeleteAsync($"http://{_rabbitMqHost}:15672/api/queues/{Vhost}/{queue}/contents");
}