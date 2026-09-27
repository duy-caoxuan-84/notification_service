using Microsoft.EntityFrameworkCore;
using NotificationService.Core.Entities;

namespace NotificationService.Infrastructure.Data;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
    {
    }

    public DbSet<NotificationRecord> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<NotificationRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.IdempotencyKey)
                  .IsRequired()
                  .HasMaxLength(100);

            // Important: Index the idempotency key to prevent duplicate notifications
            entity.HasIndex(e => e.IdempotencyKey)
                  .IsUnique();

            entity.Property(e => e.UserId)
                  .IsRequired()
                  .HasMaxLength(50);
                  
            // Save Enums as strings in PostgreSQL for readability
            entity.Property(e => e.Channel)
                  .HasConversion<string>()
                  .HasMaxLength(20);

            entity.Property(e => e.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20);
        });
    }
}
