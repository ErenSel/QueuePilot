namespace QueuePilot.Domain.Entities;

public class ProcessedEvent
{
    public Guid Id { get; set; } // Primary Key (can be EventId if unique)
    public string EventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public string ProcessorName { get; set; } = string.Empty;

    private ProcessedEvent() { }

    public static ProcessedEvent Create(string eventId, string eventType, string processorName)
    {
        return new ProcessedEvent
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            EventType = eventType,
            ProcessorName = processorName,
            ProcessedAt = DateTime.UtcNow
        };
    }
}
