using System.Data;
using System.Diagnostics.CodeAnalysis;
using FinanceManager.TransportLibrary.Models;
using FinanceManager.TransportLibrary.Services;
using FinanceManager.TransportLibrary.Services.Outbox;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using RabbitMQ.Client;
using RedLockNet;

namespace FinanceManager.TransportLibrary.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    private const int RetryDelay = 10000;
    private static readonly List<TransportInfo> TransportMap = new();
    private static bool _outboxEnabled;

    public static IServiceCollection AddCommon(this IServiceCollection services)
    {
        services.AddSingleton<IDistributedLockFactory, GlobalLockService>();

        return services;
    }

    public static IServiceCollection AddTransportCore(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var settings = new MessageTransportSettings();
        configuration.GetSection(MessageTransportSettings.SectionName).Bind(settings);
        services.AddSingleton(settings);
        var outboxSetting = new OutboxSettings();
        configuration.GetSection(OutboxSettings.SectionName).Bind(outboxSetting);
        _outboxEnabled = outboxSetting.Enabled;
        services.AddSingleton(outboxSetting);
        if (_outboxEnabled)
        {
            services.AddTransient<IDbConnection>(_ => new NpgsqlConnection(configuration.GetConnectionString("DefaultConnection")));
            services.AddTransient<IMessageRepository, MessageRepository>();
            services.AddTransient<IOutboxDbMigrator, OutboxDbMigrator>();
            services.AddTransient<IGetWaitTimeForNextOutboxCallService, GetWaitTimeForNextOutboxCallService>();
            services.AddScoped<IOutboxSessionService, OutboxSessionService>();
            services.AddHostedService<OutboxBackgroundService>();
        }

        ConnectionFactory connectionFactory = new ConnectionFactory
        {
            HostName = settings.Hostname,
            UserName = settings.User,
            Password = settings.Password,
            DispatchConsumersAsync = true
        };
        var connection = connectionFactory.CreateConnection();
        var channel = connection.CreateModel();
        // accept only one unack-ed message at a time
        // uint prefetchSize, ushort prefetchCount, bool global
        channel.BasicQos(0, 1, false);

        services.AddSingleton(channel);
        return services;
    }

    public static IServiceProvider BuildTransportMap(this IServiceProvider serviceProvider)
    {
        var channel = serviceProvider.GetRequiredService<IModel>();
        foreach (var info in TransportMap)
        {
            var deadLetterExchangeName = $"dead-letter-{info.ExchangeName}";
            var deadLetterQueueName = $"dead-letter-{info.QueueName}";

            channel.QueueDeclare(deadLetterQueueName, false, false, false, new Dictionary<string, object>
            {
                {"x-dead-letter-exchange", info.ExchangeName},
                {"x-dead-letter-routing-key", info.EventName},
                {"x-max-length", 50000},
                {"x-overflow", "reject-publish"},
                {"x-message-ttl", RetryDelay}
            });
            channel.ExchangeDeclare(deadLetterExchangeName, ExchangeType.Topic);

            channel.QueueBind(deadLetterQueueName, deadLetterExchangeName, info.EventName);

            channel.ExchangeDeclare(info.ExchangeName, ExchangeType.Direct);
            
            channel.QueueDeclare(info.QueueName, false, false, false, new Dictionary<string, object>
            {
                {"x-dead-letter-exchange", deadLetterExchangeName}
            });

            channel.QueueBind(info.QueueName, info.ExchangeName, info.EventName);
            
            var consumer = serviceProvider.GetRequiredService(info.Consumer) as IBasicConsumer;
            if (consumer == null)
            {
                throw new ArgumentException("Run consumers exception");
            }

            channel.BasicConsume(
                queue: info.QueueName,
                autoAck: false,
                consumer: consumer);
        }

        if (_outboxEnabled)
        {
            using var migrator = serviceProvider.GetRequiredService<IOutboxDbMigrator>();
            migrator.Migrate();
        }

        return serviceProvider;
    }

    public static IServiceCollection AddTransportConsumerWithReject<TMessage, TRejectMessage, TMessageConsumer>(
        this IServiceCollection services,
        string rejectExchangeName,
        string exchangeName,
        string queueName)
        where TMessageConsumer : class, IMessageConsumer<TMessage>
        where TMessage : IRejectableEvent
        where TRejectMessage : IEvent
    {
        services.AddTransportPublisher<TRejectMessage>(rejectExchangeName);
        services.AddTransportConsumer<TMessage, TMessageConsumer>(exchangeName, queueName);
        return services;
    }

    public static IServiceCollection AddTransportConsumer<TMessage, TMessageConsumer>(
        this IServiceCollection services,
        string exchangeName,
        string queueName) where TMessageConsumer : class, IMessageConsumer<TMessage>
    {
        var eventName = typeof(TMessage).Name;
        services.AddTransient<IMessageConsumer<TMessage>, TMessageConsumer>();
        TransportMap.Add(new TransportInfo(
            exchangeName,
            queueName,
            eventName,
            typeof(IMessageConsumer<TMessage>)));

        return services;
    }

    public static IServiceCollection AddTransportPublisher<TMessage>(
        this IServiceCollection services,
        string exchangeName)
    {
        var eventName = typeof(TMessage).Name;
        services.AddTransient<MessagePublisher<TMessage>>(p =>
        {
            var channel = p.GetRequiredService<IModel>();
            return new MessagePublisher<TMessage>(channel, exchangeName, eventName);
        });
        services.AddTransient<IMessagePublisher<TMessage>>(p =>
        {
            var channel = p.GetRequiredService<IModel>();
            return new MessagePublisher<TMessage>(channel, exchangeName, eventName);
        });
        services.Decorate<IMessagePublisher<TMessage>, CircuitBreakerMessagePublisherDecorator<TMessage>>();
        if (_outboxEnabled)
        {
            services.Decorate<IMessagePublisher<TMessage>, OutboxMessagePublisherDecorator<TMessage>>();
        }
        return services;
    }
}