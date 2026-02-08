using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using QueuePilot.Application.Common.Interfaces;
using QueuePilot.Domain.Common;
using QueuePilot.Domain.Events;
using RabbitMQ.Client;

namespace QueuePilot.Infrastructure.Messaging;

public class RabbitMQEventBus : IEventBus, IDisposable
{
    private readonly RabbitMQOptions _options;
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMQEventBus(IOptions<RabbitMQOptions> options)
    {
        _options = options.Value;
        InitializeRabbitMQ();
    }

    private void InitializeRabbitMQ()
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            DispatchConsumersAsync = true
        };

        try
        {
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            
            _channel.ExchangeDeclare(exchange: _options.ExchangeName, type: ExchangeType.Topic, durable: true);
        }
        catch (Exception ex)
        {
            // Log failure, retry logic or fallback handled by caller/resilience policy
            Console.WriteLine($"Could not connect to RabbitMQ: {ex.Message}");
        }
    }

    public Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        if (_channel == null || _channel.IsClosed)
        {
            // Re-connect logic or throw
            return Task.CompletedTask;
        }

        var routingKey = GetRoutingKey(domainEvent);
        var payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType());
        var body = Encoding.UTF8.GetBytes(payload);

        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.MessageId = Guid.NewGuid().ToString();
        properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        
        // Add type header for deserialization
        properties.Headers = new Dictionary<string, object>
        {
            { "EventType", domainEvent.GetType().Name }
        };

        _channel.BasicPublish(
            exchange: _options.ExchangeName,
            routingKey: routingKey,
            basicProperties: properties,
            body: body);

        return Task.CompletedTask;
    }

    private string GetRoutingKey(IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            TicketCreatedEvent => "ticket.created",
            TicketStatusChangedEvent => "ticket.status.changed",
            TicketCommentAddedEvent => "ticket.comment.added",
            _ => "ticket.event.general"
        };
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
}
