using MediatR;
using QueuePilot.Application.Common.Interfaces;
using QueuePilot.Domain.Entities;

namespace QueuePilot.Application.Tickets.Commands;

public record CreateTicketCommand(Guid CustomerId, string Title, string Description) : IRequest<Guid>;

public class CreateTicketCommandHandler : IRequestHandler<CreateTicketCommand, Guid>
{
    private readonly IQueuePilotDbContext _context;
    private readonly IEventBus _eventBus;

    public CreateTicketCommandHandler(IQueuePilotDbContext context, IEventBus eventBus)
    {
        _context = context;
        _eventBus = eventBus;
    }

    public async Task<Guid> Handle(CreateTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = Ticket.Create(request.Title, request.Description, request.CustomerId);

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync(cancellationToken); 
        
        foreach(var domainEvent in ticket.DomainEvents)
        {
            await _eventBus.PublishAsync(domainEvent, cancellationToken);
        }

        return ticket.Id;
    }
}
