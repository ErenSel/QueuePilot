using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using QueuePilot.Domain.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace QueuePilot.Infrastructure.Messaging;

public class EventConsumerService : BackgroundService
{
    private readonly RabbitMQOptions _options;
    private readonly ILogger<EventConsumerService> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    private readonly IServiceProvider _serviceProvider;

    public EventConsumerService(
        IOptions<RabbitMQOptions> options,
        ILogger<EventConsumerService> logger,
        IServiceProvider serviceProvider)
    {
        _options = options.Value;
        _logger = logger;
        _serviceProvider = serviceProvider;
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

            // DLQ Setup
            var dlqName = "queuepilot.dlq";
            _channel.QueueDeclare(queue: dlqName, durable: true, exclusive: false, autoDelete: false);

            var args = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", "" }, // Default exchange
                { "x-dead-letter-routing-key", dlqName }
            };

            // Declare Queues with DLQ
            _channel.QueueDeclare(queue: "queuepilot.notifications", durable: true, exclusive: false, autoDelete: false, arguments: args);
            _channel.QueueDeclare(queue: "queuepilot.audit", durable: true, exclusive: false, autoDelete: false, arguments: args);

            // Bindings
            _channel.QueueBind(queue: "queuepilot.notifications", exchange: _options.ExchangeName, routingKey: "ticket.#");
            _channel.QueueBind(queue: "queuepilot.audit", exchange: _options.ExchangeName, routingKey: "#");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not initialize RabbitMQ Consumer connection");
        }
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_channel == null) return Task.CompletedTask;

        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var eventTypeHeader = ea.BasicProperties.Headers["EventType"] as byte[];
            var eventType = eventTypeHeader != null ? Encoding.UTF8.GetString(eventTypeHeader) : "Unknown";

            _logger.LogInformation("Received Event: {EventType} | RoutingKey: {Key}", eventType, ea.RoutingKey);

            try
            {
                // Basic properties access for MessageId
                var messageId = ea.BasicProperties.MessageId;
                if(string.IsNullOrEmpty(messageId)) messageId = Guid.NewGuid().ToString();

                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<Persistence.AppDbContext>();
                    
                    // Idempotency Check
                    if (await dbContext.ProcessedEvents.AnyAsync(e => e.EventId == messageId))
                    {
                        _logger.LogInformation("Event {EventId} already processed. Skipping.", messageId);
                        _channel.BasicAck(ea.DeliveryTag, false);
                        return;
                    }

                    await ProcessEvent(eventType, message);

                    // Save Processed Event
                    dbContext.ProcessedEvents.Add(Domain.Entities.ProcessedEvent.Create(messageId, eventType, "EventConsumerService"));
                    await dbContext.SaveChangesAsync();
                }

                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
                _channel.BasicNack(ea.DeliveryTag, false, true); // Requeue
            }
        };

        _channel.BasicConsume(queue: "queuepilot.notifications", autoAck: false, consumer: consumer);
        
        return Task.CompletedTask;
    }

    private Task ProcessEvent(string eventType, string message)
    {
        // Simulation of processing
        switch (eventType)
        {
            case nameof(TicketCreatedEvent):
                var createdEvent = JsonSerializer.Deserialize<TicketCreatedEvent>(message);
                _logger.LogInformation(">>> NOTIFICATION: New Ticket Created! ID: {TicketId}, Title: {Title}", createdEvent?.TicketId, createdEvent?.Title);
                break;
            case nameof(TicketStatusChangedEvent):
                var statusEvent = JsonSerializer.Deserialize<TicketStatusChangedEvent>(message);
                _logger.LogInformation(">>> NOTIFICATION: Ticket Status Changed! ID: {TicketId}, NewStatus: {Status}", statusEvent?.TicketId, statusEvent?.NewStatus);
                break;
             case nameof(TicketCommentAddedEvent):
                var commentEvent = JsonSerializer.Deserialize<TicketCommentAddedEvent>(message);
                _logger.LogInformation(">>> NOTIFICATION: New Comment on Ticket {TicketId}", commentEvent?.TicketId);
                break;
            default:
                _logger.LogWarning("Unknown event type: {Type}", eventType);
                break;
        }

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}
