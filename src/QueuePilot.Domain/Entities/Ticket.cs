using QueuePilot.Domain.Common;
using QueuePilot.Domain.Enums;

namespace QueuePilot.Domain.Entities;

public class Ticket : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public TicketStatus Status { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid? AssignedAgentId { get; private set; }

    private readonly List<TicketComment> _comments = new();
    public IReadOnlyCollection<TicketComment> Comments => _comments.AsReadOnly();

    private Ticket() { }

    public static Ticket Create(string title, string description, Guid customerId)
    {
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            Status = TicketStatus.Open,
            CustomerId = customerId,
            CreatedAt = DateTime.UtcNow
        };
        
        ticket.AddDomainEvent(new Events.TicketCreatedEvent(ticket.Id, ticket.CustomerId, ticket.Title));
        return ticket;
    }

    public void AssignTo(Guid agentId)
    {
        AssignedAgentId = agentId;
        Status = TicketStatus.InProgress;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new Events.TicketStatusChangedEvent(Id, TicketStatus.Open, TicketStatus.InProgress)); // Simplified old status
    }

    public void ChangeStatus(TicketStatus newStatus)
    {
        // Add validation logic if needed (e.g. Closed cannot go back to Open)
        var oldStatus = Status;
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new Events.TicketStatusChangedEvent(Id, oldStatus, newStatus));
    }

    public void AddComment(string text, Guid userId)
    {
        var comment = TicketComment.Create(Id, userId, text);
        _comments.Add(comment);
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new Events.TicketCommentAddedEvent(Id, comment.Id, userId));
    }
}

public class TicketComment : BaseEntity
{
    public Guid TicketId { get; private set; }
    public Guid UserId { get; private set; } // Author
    public string Text { get; private set; } = string.Empty;

    private TicketComment() { }

    public static TicketComment Create(Guid ticketId, Guid userId, string text)
    {
        return new TicketComment
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            UserId = userId,
            Text = text,
            CreatedAt = DateTime.UtcNow
        };
    }
}
