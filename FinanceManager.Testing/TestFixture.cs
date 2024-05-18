using System.Net;
using FinanceManager.Testing.Clients;
using FinanceManager.Testing.Helpers;
using FinanceManager.Testing.Helpers.JwtAuthorisation;
using FinanceManager.Testing.Helpers.JwtAuthorisation.Models;
using MbDotNet.Models;
using Xunit;
using Xunit.Abstractions;

namespace FinanceManager.Testing;

public abstract class TestFixture : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    protected const int AuthPort = 5194;
    protected const string AuthSecurityKey = "!SomethingSecret!";
    protected string AuthUrl { get; } = $"http://localhost:{AuthPort}";
    protected const string ApiKey = "!SecureApiKey!";
    protected abstract string AuthScope { get; }

    protected TestFixture(ITestOutputHelper output)
    {
        _output = output;
    }

    protected abstract Task StartAsync();
    public virtual async Task InitializeAsync()
    {
        await StartAsync();
        await ExtEnvironment.MountebankClient.CreateHttpImposterAsync(AuthPort, imposter =>
        {
            imposter.RecordRequests = true;
            imposter.AllowCORS = true;
            imposter.AddStub()
                .OnPathAndMethodEqual("/.well-known/openid-configuration", Method.Get)
                .ReturnsJson(HttpStatusCode.OK, AuthResponseGenerator.GetOpenidConfiguration(AuthUrl));
            imposter.AddStub()
                .OnPathAndMethodEqual("/.well-known/openid-configuration/jwks", Method.Get)
                .ReturnsJson(HttpStatusCode.OK, AuthResponseGenerator.GetCertificates());
            imposter.AddStub()
                .OnPathAndMethodEqual("/connect/token", Method.Post)
                .ReturnsJson(HttpStatusCode.OK,
                    AuthResponseGenerator.GetToken(
                        AuthScope,
                        AuthSecurityKey,
                        new AuthorizationUser("1", "admin", "admin@mail.com"))
                );
        });
    }

    public async Task DisposeAsync()
    {
        await ExtEnvironment.ClearContainers();
    }
    protected async Task<IRestServiceClient> GetRestClient(bool useTokenAuth)
    {
        var auth = useTokenAuth ? $"Bearer {await AuthAsync()}" : null;
        return new RestServiceClient(new DefaultHttpClient(ExtEnvironment.TestServer.CreateClient()), _output, auth);
    }

    protected async Task<string> AuthAsync()
    {
        using var httpClient = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, $"{AuthUrl}/connect/token");
        var response = await httpClient.SendAsync(request);
        return response.Content.ReadAs<Token>()!.AccessToken;
    }
}