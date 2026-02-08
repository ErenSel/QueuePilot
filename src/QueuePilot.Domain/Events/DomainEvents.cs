using QueuePilot.Domain.Common;
using QueuePilot.Domain.Enums;

namespace QueuePilot.Domain.Events;

public record TicketCreatedEvent(Guid TicketId, Guid CustomerId, string Title) : IDomainEvent;
public record TicketStatusChangedEvent(Guid TicketId, TicketStatus OldStatus, TicketStatus NewStatus) : IDomainEvent;
public record TicketCommentAddedEvent(Guid TicketId, Guid CommentId, Guid UserId) : IDomainEvent;
