using Docker.DotNet;
using Docker.DotNet.Models;

namespace FinanceManager.Testing.Containers;

public class RabbitmqContainer : BaseContainer
{
    private readonly string _rabbitmqHost;
    private readonly string _rabbitmqUser;
    private readonly string _rabbitmqPassword;

    public RabbitmqContainer(
        string rabbitmqHost,
        string rabbitmqUser,
        string rabbitmqPassword,
        DockerClient dockerClient) : base("rabbitmq", "3.13-management", dockerClient)
    {
        _rabbitmqHost = rabbitmqHost;
        _rabbitmqUser = rabbitmqUser;
        _rabbitmqPassword = rabbitmqPassword;
    }

    public override async Task StartContainer()
    {
        await PullImage(Image,Tag);
            
        var exposedPorts = new Dictionary<string, EmptyStruct>
        {
            {
                "5672", default
            },
            {
                "15672", default
            },
        };

        var portBindings = new Dictionary<string, IList<PortBinding>>
        {
            {"5672", new List<PortBinding> {new() {HostPort = "5672"}}},
            {"15672", new List<PortBinding> {new() {HostPort = "15672"}}}
        };

        var env = new List<string>
        {
            $"RABBITMQ_DEFAULT_USER={_rabbitmqUser}",
            $"RABBITMQ_DEFAULT_PASS={_rabbitmqPassword}",
        };
            
        var container = await DockerClient.Containers.CreateContainerAsync(new CreateContainerParameters
        {
            Image = ImageFull,
            Env = env,
            ExposedPorts = exposedPorts,
            HostConfig = new HostConfig
            {
                PortBindings = portBindings,
                PublishAllPorts = true
            },
        });
            
        ContainerId = container.ID;
        await DockerClient.Containers.StartContainerAsync(ContainerId, null);
        await WaitContainer();
    }

    protected override async Task WaitContainer()
    {
        for (var i = 0; i < 30; i++)
        {
            try
            {
                using var client = new HttpClient();
                var response = await client.GetAsync($"http://{_rabbitmqHost}:15672");
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (Exception)
            {
                //ignore
            }
                
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }
}