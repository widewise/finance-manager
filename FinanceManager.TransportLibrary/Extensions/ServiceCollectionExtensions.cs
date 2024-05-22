using System.Diagnostics.CodeAnalysis;
using FinanceManager.TransportLibrary.Models;
using FinanceManager.TransportLibrary.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace FinanceManager.TransportLibrary.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    private static readonly List<TransportInfo> TransportMap = new();

    public static IServiceCollection AddTransportCore(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var settings = new MessageTransportSettings();
        configuration.GetSection(MessageTransportSettings.SectionName).Bind(settings);
        services.AddSingleton(settings);

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
            channel.ExchangeDeclare(info.ExchangeName, ExchangeType.Direct);
            channel.QueueDeclare(info.QueueName, false, false, false, null);
            channel.QueueBind(info.QueueName, info.ExchangeName, info.EventName);
            var consumer = serviceProvider.GetRequiredService(info.Consumer) as IBasicConsumer;
            if (consumer == null)
            {
                throw new Exception("Run consumers exception");
            }

            channel.BasicConsume(
                queue: info.QueueName,
                autoAck: false,
                consumer: consumer);
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
        services.AddTransient<IMessagePublisher<TMessage>>(p =>
        {
            var channel = p.GetRequiredService<IModel>();
            return new MessagePublisher<TMessage>(channel, exchangeName, eventName);
        });
        return services;
    }
}