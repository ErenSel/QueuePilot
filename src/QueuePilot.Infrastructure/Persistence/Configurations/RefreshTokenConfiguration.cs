using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QueuePilot.Domain.Entities;

namespace QueuePilot.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshToken");

        builder.HasKey(t => t.Id); // Inherited from BaseEntity but good to be explicit

        builder.Property(t => t.Token)
            .IsRequired();
            
        // Relationship is configured in UserConfiguration, or can be here.
        // If configured here:
        // builder.HasOne<User>() // We don't have User navigation
        //     .WithMany(u => u.RefreshTokens)
        //     .HasForeignKey(t => t.UserId)
        //     .OnDelete(DeleteBehavior.Cascade);
    }
}
