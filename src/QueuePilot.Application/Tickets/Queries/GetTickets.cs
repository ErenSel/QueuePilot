using MediatR;
using Microsoft.EntityFrameworkCore;
using QueuePilot.Application.Common.Interfaces;
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

public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize);

public class GetTicketsQueryHandler : IRequestHandler<GetTicketsQuery, PagedResult<TicketDto>>
{
    private readonly IQueuePilotDbContext _context;

    public GetTicketsQueryHandler(IQueuePilotDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<TicketDto>> Handle(GetTicketsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Tickets.AsNoTracking().AsQueryable();

        if (request.Status.HasValue)
        {
            query = query.Where(t => t.Status == request.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(t => t.Title.ToLower().Contains(term) || t.Description.ToLower().Contains(term));
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= request.ToDate.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var tickets = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new TicketDto(t.Id, t.Title, t.Status.ToString(), t.CreatedAt, t.Description))
            .ToListAsync(cancellationToken);

        return new PagedResult<TicketDto>(tickets, totalCount, request.Page, request.PageSize);
    }
}
