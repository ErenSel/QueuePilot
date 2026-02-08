using Microsoft.EntityFrameworkCore;
using QueuePilot.Domain.Entities;

namespace QueuePilot.Application.Common.Interfaces;

public interface IQueuePilotDbContext
{
    DbSet<User> Users { get; }
    DbSet<Ticket> Tickets { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
