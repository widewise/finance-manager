using Docker.DotNet;
using Docker.DotNet.Models;

namespace FinanceManager.Testing.Containers;

public abstract class BaseContainer
{
    protected readonly DockerClient DockerClient;
    protected string? ContainerId;
        
    protected readonly string Image;
    protected readonly string Tag;

    protected string ImageFull => $"{Image}:{Tag}";
        
    protected BaseContainer(string image, string tag, DockerClient dockerClient)
    {
        Image = image;
        Tag = tag;
        DockerClient = dockerClient;
    }

    protected async Task PullImage(string image,string tag)
    {
        await DockerClient.Images
            .CreateImageAsync(new ImagesCreateParameters
                {
                    FromImage = image,
                    Tag = tag
                },
                new AuthConfig(),
                new Progress<JSONMessage>());
    }

    public abstract Task StartContainer();
    protected abstract Task WaitContainer();
}