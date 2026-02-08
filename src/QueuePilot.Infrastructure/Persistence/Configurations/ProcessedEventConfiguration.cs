using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QueuePilot.Domain.Entities;

namespace QueuePilot.Infrastructure.Persistence.Configurations;

public class ProcessedEventConfiguration : IEntityTypeConfiguration<ProcessedEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedEvent> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.EventId)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.HasIndex(e => new { e.EventId, e.ProcessorName })
            .IsUnique();
    }
}
