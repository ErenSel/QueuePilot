using MediatR;
using QueuePilot.Application.Common.Events;
using QueuePilot.Application.Common.Exceptions;
using QueuePilot.Application.Common.Interfaces;

namespace QueuePilot.Application.Tickets.Commands;

public record AddTicketCommentCommand(Guid TicketId, Guid UserId, string Text) : IRequest;

public class AddTicketCommentCommandHandler : IRequestHandler<AddTicketCommentCommand>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventPublisher _domainEventPublisher;

    public AddTicketCommentCommandHandler(
        ITicketRepository ticketRepository,
        IUnitOfWork unitOfWork,
        IDomainEventPublisher domainEventPublisher)
    {
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
        _domainEventPublisher = domainEventPublisher;
    }

    public async Task Handle(AddTicketCommentCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);

        if (ticket == null)
             throw new NotFoundException($"Ticket {request.TicketId} not found");

        ticket.AddComment(request.Text, request.UserId);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach(var domainEvent in ticket.DomainEvents)
        {
            await _domainEventPublisher.PublishAsync(domainEvent, cancellationToken);
        }
    }
}
