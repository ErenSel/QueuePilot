using QueuePilot.Domain.Common;

namespace QueuePilot.Application.Common.Events;

public interface IDomainEventPublisher
{
    Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
