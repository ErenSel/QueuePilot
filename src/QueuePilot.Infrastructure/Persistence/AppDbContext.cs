using Microsoft.EntityFrameworkCore;
using QueuePilot.Application.Common.Interfaces;
using QueuePilot.Domain.Entities;

namespace QueuePilot.Infrastructure.Persistence;

public class AppDbContext : DbContext, IQueuePilotDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<ProcessedEvent> ProcessedEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
