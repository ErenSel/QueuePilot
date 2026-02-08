using MediatR;
using QueuePilot.Domain.Common;

namespace QueuePilot.Application.Common.Events;

public record DomainEventNotification(IDomainEvent DomainEvent) : INotification;
