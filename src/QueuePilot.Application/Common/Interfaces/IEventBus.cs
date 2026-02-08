using QueuePilot.Domain.Common;

namespace QueuePilot.Application.Common.Interfaces;

public interface IEventBus
{
    Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
