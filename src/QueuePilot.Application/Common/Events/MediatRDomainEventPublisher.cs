using MediatR;
using QueuePilot.Domain.Common;

namespace QueuePilot.Application.Common.Events;

public class MediatRDomainEventPublisher : IDomainEventPublisher
{
    private readonly IMediator _mediator;

    public MediatRDomainEventPublisher(IMediator mediator)
    {
        _mediator = mediator;
    }

    public Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        return _mediator.Publish(new DomainEventNotification(domainEvent), cancellationToken);
    }
}
