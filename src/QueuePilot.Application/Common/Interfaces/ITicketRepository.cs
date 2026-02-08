using QueuePilot.Application.Common.Models;
using QueuePilot.Domain.Entities;

namespace QueuePilot.Application.Common.Interfaces;

public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(Ticket ticket, CancellationToken cancellationToken);
    Task UpdateAsync(Ticket ticket, CancellationToken cancellationToken);
    Task<PagedResult<Ticket>> ListAsync(TicketQueryParameters parameters, CancellationToken cancellationToken);
}
