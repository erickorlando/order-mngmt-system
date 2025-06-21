using System.Text;
using System.Text.Json;
using EventBus.Events;
using EventBus.Interfaces;
using RabbitMQ.Client;

namespace EventBus.RabbitMQ;

public class RabbitMqEventBus : IEventBus, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _exchangeName;
    private readonly string _queueName;

    public RabbitMqEventBus(string hostName, string userName, string password, string exchangeName, string queueName)
    {
        _exchangeName = exchangeName;
        _queueName = queueName;

        var factory = new ConnectionFactory
        {
            HostName = hostName,
            UserName = userName,
            Password = password
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Declarar exchange y queue
        _channel.ExchangeDeclare(_exchangeName, ExchangeType.Topic, durable: true);
        _channel.QueueDeclare(_queueName, durable: true, exclusive: false, autoDelete: false);
    }

    public async Task PublishAsync<T>(T @event) where T : IntegrationEvent
    {
        var eventName = typeof(T).Name;
        var message = JsonSerializer.Serialize(@event);
        var body = Encoding.UTF8.GetBytes(message);

        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.MessageId = @event.Id.ToString();
        properties.Timestamp = new AmqpTimestamp(((DateTimeOffset)@event.CreatedAt).ToUnixTimeSeconds());

        _channel.BasicPublish(
            exchange: _exchangeName,
            routingKey: eventName,
            basicProperties: properties,
            body: body);

        await Task.CompletedTask;
    }

    public async Task SubscribeAsync<T, THandler>() 
        where T : IntegrationEvent 
        where THandler : class, IIntegrationEventHandler<T>
    {
        var eventName = typeof(T).Name;
        _channel.QueueBind(_queueName, _exchangeName, eventName);
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}