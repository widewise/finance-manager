using System.Runtime.InteropServices;
using Docker.DotNet;
using Docker.DotNet.Models;
using FinanceManager.Testing.Containers;
using FinanceManager.Testing.Helpers.Clients.RabbitMq;
using MbDotNet;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace FinanceManager.Testing;

public class ExtEnvironment
{
    private static DockerClient? _dockerClient;
    public static PostgresContainer PostgresContainer { get; set; } = null!;
    public static MountebankContainer MountebankContainer { get; set; } = null!;
    public static RabbitmqContainer RabbitmqContainer { get; set; } = null!;
    public static TestServer TestServer { get; private set; } = null!;
    public static MountebankClient MountebankClient { get; private set; } = new(new Uri("http://localhost:2525"));
    public static RabbitMqClient RabbitMqClient { get; private set; } = null!;

    public static async Task Start<TEntryPoint>(
        int authPort,
        string authUrl,
        string authKey,
        string apiKey) where TEntryPoint : class
    {
        var dbConnectionString = "Host=localhost:5432;Database=test-db;Username=postgres;Password=postgres";
        var transportHost = "localhost";
        var transportUser = "guest";
        var transportPassword = "guest";

        _dockerClient = new DockerClientConfiguration(new Uri(DockerApiUri())).CreateClient();

        PostgresContainer = new PostgresContainer(dbConnectionString, _dockerClient);
        RabbitmqContainer = new RabbitmqContainer(transportHost, transportUser, transportPassword, _dockerClient);
        MountebankContainer = new MountebankContainer(_dockerClient);
        MountebankContainer.AddImposterPost(authPort);
        RabbitMqClient = new RabbitMqClient(transportHost, transportUser, transportPassword);

        await RemoveAllContainers(_dockerClient);

        await Task.WhenAll(PostgresContainer.StartContainer(), RabbitmqContainer.StartContainer(),
            MountebankContainer.StartContainer());

        TestServer = CreateServer<TEntryPoint>(
            dbConnectionString,
            authKey,
            authUrl,
            apiKey,
            transportHost,
            transportUser,
            transportPassword);
    }

    public static async Task ClearContainers()
    {
        if (_dockerClient != null)
        {
            await RemoveAllContainers(_dockerClient);
        }
    }

    private static TestServer CreateServer<TEntryPoint>(
        string dbConnectionString,
        string authKey,
        string authUrl,
        string apiKey,
        string transportHost,
        string transportUser,
        string transportPassword) where TEntryPoint : class
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", dbConnectionString);
        Environment.SetEnvironmentVariable("CustomAuthenticationSettings__Key", authKey);
        Environment.SetEnvironmentVariable("CustomAuthenticationSettings__IdentityUrl", authUrl + "/");
        Environment.SetEnvironmentVariable("ApiKeySettings__ApiKey", apiKey);
        Environment.SetEnvironmentVariable("MessageTransport__Hostname", transportHost);
        Environment.SetEnvironmentVariable("MessageTransport__User", transportUser);
        Environment.SetEnvironmentVariable("MessageTransport__Password", transportPassword);

        var factory = new WebApplicationFactory<TEntryPoint>();
        return factory.Server;
    }

    private static string DockerApiUri()
    {
        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        if (isWindows)
        {
            return "npipe://./pipe/docker_engine";
        }

        var isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        if (isLinux)
        {
            return "tcp://127.0.0.1:2375";
        }

        var isOsx = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        if (isOsx)
        {
            return "unix:///var/run/docker.sock";
        }

        throw new Exception("Was unable to determine what OS this is running on");
    }
    private static async Task RemoveAllContainers(DockerClient dockerClient)
    {
        IList<ContainerListResponse> containers = await dockerClient.Containers.ListContainersAsync(new ContainersListParameters());
        foreach (var container in containers)
        {
            await dockerClient.Containers.KillContainerAsync(container.ID, new ContainerKillParameters());
            await dockerClient.Containers.RemoveContainerAsync(container.ID, new ContainerRemoveParameters());
        }
    }
}