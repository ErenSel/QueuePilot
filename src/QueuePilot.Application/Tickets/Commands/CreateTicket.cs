using MediatR;
using QueuePilot.Application.Common.Events;
using QueuePilot.Application.Common.Interfaces;
using QueuePilot.Domain.Entities;

namespace QueuePilot.Application.Tickets.Commands;

public record CreateTicketCommand(Guid CustomerId, string Title, string Description) : IRequest<Guid>;

public class CreateTicketCommandHandler : IRequestHandler<CreateTicketCommand, Guid>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventPublisher _domainEventPublisher;

    public CreateTicketCommandHandler(
        ITicketRepository ticketRepository,
        IUnitOfWork unitOfWork,
        IDomainEventPublisher domainEventPublisher)
    {
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
        _domainEventPublisher = domainEventPublisher;
    }

    public async Task<Guid> Handle(CreateTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = Ticket.Create(request.Title, request.Description, request.CustomerId);

        await _ticketRepository.AddAsync(ticket, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken); 
        
        foreach(var domainEvent in ticket.DomainEvents)
        {
            await _domainEventPublisher.PublishAsync(domainEvent, cancellationToken);
        }

        return ticket.Id;
    }
}
