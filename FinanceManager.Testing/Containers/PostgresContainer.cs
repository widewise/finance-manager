using System.Data;
using Docker.DotNet;
using Docker.DotNet.Models;
using Npgsql;

namespace FinanceManager.Testing.Containers;

public class PostgresContainer : BaseContainer
{
    private readonly string _connectionString;
    private readonly NpgsqlConnectionStringBuilder _connectionStringBuilder;

    public PostgresContainer(
        string connectionString,
        DockerClient dockerClient) : base("postgres", "latest", dockerClient)
    {
        _connectionString = connectionString;
        _connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);
    }

    public override async Task StartContainer()
    {
        await PullImage(Image,Tag);
        
        var exposedPorts = new Dictionary<string, EmptyStruct>
        {
            {
                "5432", default(EmptyStruct)
            }
        };

        var portBindings = new Dictionary<string, IList<PortBinding>>
        {
            {"5432", new List<PortBinding> {new() {HostPort = "5432"}}}
        };

        var env = new List<string>
        {
            $"POSTGRES_PASSWORD={_connectionStringBuilder.Password}",
            $"POSTGRES_USER={_connectionStringBuilder.Username}"
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
            }
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
                var connectionString = GetConnectionString(null);
                await using NpgsqlConnection connection = new NpgsqlConnection(connectionString);
                connection.Open();
                if (connection.State == ConnectionState.Open)
                {
                    await connection.CloseAsync();
                    return;
                }
            }
            catch
            {
                //ignore
            }
            
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }

    private string GetConnectionString(string? dbName)
    {
        var connectionStringBuilder = new NpgsqlConnectionStringBuilder
        {
            Host = _connectionStringBuilder.Host,
            Port = 5432,
            Database =  dbName,
            Username = _connectionStringBuilder.Username,
            Password = _connectionStringBuilder.Password
        };

        return connectionStringBuilder.ConnectionString;
    }
}