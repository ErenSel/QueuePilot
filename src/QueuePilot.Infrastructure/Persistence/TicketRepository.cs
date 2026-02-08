using Microsoft.EntityFrameworkCore;
using QueuePilot.Application.Common.Interfaces;
using QueuePilot.Application.Common.Models;
using QueuePilot.Domain.Entities;

namespace QueuePilot.Infrastructure.Persistence;

public class TicketRepository : ITicketRepository
{
    private readonly AppDbContext _context;

    public TicketRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Ticket?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Tickets.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task AddAsync(Ticket ticket, CancellationToken cancellationToken)
    {
        await _context.Tickets.AddAsync(ticket, cancellationToken);
    }

    public async Task<PagedResult<Ticket>> GetPagedAsync(TicketQueryParameters parameters, CancellationToken cancellationToken)
    {
        var query = _context.Tickets.AsNoTracking().AsQueryable();

        if (parameters.Status.HasValue)
        {
            query = query.Where(t => t.Status == parameters.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            var term = parameters.SearchTerm.ToLower();
            query = query.Where(t => t.Title.ToLower().Contains(term) || t.Description.ToLower().Contains(term));
        }

        if (parameters.FromDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= parameters.FromDate.Value);
        }

        if (parameters.ToDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= parameters.ToDate.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Ticket>(items, totalCount, parameters.Page, parameters.PageSize);
    }
}
