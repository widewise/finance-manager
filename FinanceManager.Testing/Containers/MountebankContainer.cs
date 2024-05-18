using Docker.DotNet;
using Docker.DotNet.Models;

namespace FinanceManager.Testing.Containers;

public class MountebankContainer : BaseContainer
{
    private readonly List<int> _imposterPosts = new();
        
    public MountebankContainer(
        DockerClient dockerClient) : base("andyrbell/mountebank", "latest", dockerClient) { }

    public void AddImposterPost(int port)
    {
        _imposterPosts.Add(port);
    }

    public override async Task StartContainer()
    {
        await PullImage(Image, Tag);

        var exposedPorts = new Dictionary<string, EmptyStruct>
        {
            {
                "2525", default(EmptyStruct)
            }
        };

        var portBindings = new Dictionary<string, IList<PortBinding>>
        {
            {"2525", new List<PortBinding> {new() {HostPort = "2525"}}}
        };

        foreach (int port in _imposterPosts)
        {
            exposedPorts.Add(port.ToString(), default);
            portBindings.Add(port.ToString(), new List<PortBinding> {new() {HostPort = port.ToString()}});
        }

        var container = await DockerClient.Containers.CreateContainerAsync(new CreateContainerParameters
        {
            Image = ImageFull,
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
        for (int i = 0; i < 30; i++)
        {
            try
            {
                using var client = new HttpClient();
                var response = await client.GetAsync("http://localhost:2525");
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (Exception)
            {
                //ignore
            }
        }
            
        await Task.Delay(TimeSpan.FromSeconds(1));
    }
}