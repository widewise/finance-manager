using RabbitMQ.Client;

namespace FinanceManager.TransportLibrary.Services;

public interface IMessageConsumer<in TMessage>: IBasicConsumer
{
    Task ExecuteEventAsync(TMessage message);
}