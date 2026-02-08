using MediatR;
using QueuePilot.Application.Common.Interfaces;
using QueuePilot.Application.Common.Models;
using QueuePilot.Domain.Entities;
using QueuePilot.Domain.Enums;

namespace QueuePilot.Application.Tickets.Queries;

public record GetTicketsQuery(
    int Page, 
    int PageSize, 
    TicketStatus? Status,
    string? SearchTerm,
    DateTime? FromDate,
    DateTime? ToDate) : IRequest<PagedResult<TicketDto>>;

public record TicketDto(Guid Id, string Title, string Status, DateTime CreatedAt, string Description);

public class GetTicketsQueryHandler : IRequestHandler<GetTicketsQuery, PagedResult<TicketDto>>
{
    private readonly ITicketRepository _ticketRepository;

    public GetTicketsQueryHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<PagedResult<TicketDto>> Handle(GetTicketsQuery request, CancellationToken cancellationToken)
    {
        var parameters = new TicketQueryParameters(
            request.Page,
            request.PageSize,
            request.Status,
            request.SearchTerm,
            request.FromDate,
            request.ToDate);

        var result = await _ticketRepository.GetPagedAsync(parameters, cancellationToken);

        var tickets = result.Items
            .Select(t => new TicketDto(t.Id, t.Title, t.Status.ToString(), t.CreatedAt, t.Description))
            .ToList();

        return new PagedResult<TicketDto>(tickets, result.TotalCount, result.Page, result.PageSize);
    }
}
