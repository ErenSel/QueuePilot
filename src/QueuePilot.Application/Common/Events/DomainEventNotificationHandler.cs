using MediatR;
using QueuePilot.Application.Common.Interfaces;

namespace QueuePilot.Application.Common.Events;

public class DomainEventNotificationHandler : INotificationHandler<DomainEventNotification>
{
    private readonly IEventBus _eventBus;

    public DomainEventNotificationHandler(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public Task Handle(DomainEventNotification notification, CancellationToken cancellationToken)
    {
        return _eventBus.PublishAsync(notification.DomainEvent, cancellationToken);
    }
}
