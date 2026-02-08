using MediatR;
using QueuePilot.Application.Common.Events;
using QueuePilot.Application.Common.Exceptions;
using QueuePilot.Application.Common.Interfaces;
using QueuePilot.Domain.Enums;

namespace QueuePilot.Application.Tickets.Commands;

public record UpdateTicketStatusCommand(Guid TicketId, TicketStatus NewStatus) : IRequest;

public class UpdateTicketStatusCommandHandler : IRequestHandler<UpdateTicketStatusCommand>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventPublisher _domainEventPublisher;

    public UpdateTicketStatusCommandHandler(
        ITicketRepository ticketRepository,
        IUnitOfWork unitOfWork,
        IDomainEventPublisher domainEventPublisher)
    {
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
        _domainEventPublisher = domainEventPublisher;
    }

    public async Task Handle(UpdateTicketStatusCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);

        if (ticket == null)
        {
            throw new NotFoundException($"Ticket {request.TicketId} not found");
        }

        ticket.ChangeStatus(request.NewStatus);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach(var domainEvent in ticket.DomainEvents)
        {
            await _domainEventPublisher.PublishAsync(domainEvent, cancellationToken);
        }
    }
}
