using MediatR;
using Microsoft.EntityFrameworkCore;
using QueuePilot.Application.Common.Interfaces;
using QueuePilot.Domain.Enums;

namespace QueuePilot.Application.Tickets.Commands;

public record UpdateTicketStatusCommand(Guid TicketId, TicketStatus NewStatus) : IRequest;

public class UpdateTicketStatusCommandHandler : IRequestHandler<UpdateTicketStatusCommand>
{
    private readonly IQueuePilotDbContext _context;
    private readonly IEventBus _eventBus;

    public UpdateTicketStatusCommandHandler(IQueuePilotDbContext context, IEventBus eventBus)
    {
        _context = context;
        _eventBus = eventBus;
    }

    public async Task Handle(UpdateTicketStatusCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _context.Tickets.FindAsync(new object[] { request.TicketId }, cancellationToken);

        if (ticket == null)
        {
            throw new Exception($"Ticket {request.TicketId} not found");
        }

        ticket.ChangeStatus(request.NewStatus);
        
        await _context.SaveChangesAsync(cancellationToken);

        foreach(var domainEvent in ticket.DomainEvents)
        {
            await _eventBus.PublishAsync(domainEvent, cancellationToken);
        }
    }
}
