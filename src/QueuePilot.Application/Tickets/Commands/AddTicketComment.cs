using MediatR;
using QueuePilot.Application.Common.Interfaces;

namespace QueuePilot.Application.Tickets.Commands;

public record AddTicketCommentCommand(Guid TicketId, Guid UserId, string Text) : IRequest;

public class AddTicketCommentCommandHandler : IRequestHandler<AddTicketCommentCommand>
{
    private readonly IQueuePilotDbContext _context;
    private readonly IEventBus _eventBus;

    public AddTicketCommentCommandHandler(IQueuePilotDbContext context, IEventBus eventBus)
    {
        _context = context;
        _eventBus = eventBus;
    }

    public async Task Handle(AddTicketCommentCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _context.Tickets.FindAsync(new object[] { request.TicketId }, cancellationToken);

        if (ticket == null)
             throw new Exception($"Ticket {request.TicketId} not found");

        ticket.AddComment(request.Text, request.UserId);
        
        await _context.SaveChangesAsync(cancellationToken);

        foreach(var domainEvent in ticket.DomainEvents)
        {
            await _eventBus.PublishAsync(domainEvent, cancellationToken);
        }
    }
}
