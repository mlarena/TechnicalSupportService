using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechnicalSupportService.Data.Entities;

namespace TechnicalSupportService.Data.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Number).IsRequired().HasMaxLength(50);
        builder.HasIndex(t => t.Number).IsUnique();
        builder.Property(t => t.Title).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Description).IsRequired();
        builder.Property(t => t.Version).HasMaxLength(20);
        builder.Property(t => t.Priority).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Category).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Impact).HasConversion<string?>().HasMaxLength(20);
        builder.Property(t => t.Source).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(t => t.Product).WithMany(p => p.Tickets)
            .HasForeignKey(t => t.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.CreatedByUser).WithMany()
            .HasForeignKey(t => t.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.AssignedToUser).WithMany()
            .HasForeignKey(t => t.AssignedToUserId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(t => t.UpdatedByUser).WithMany()
            .HasForeignKey(t => t.UpdatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.ParentTicket).WithMany(t => t.ChildTickets)
            .HasForeignKey(t => t.ParentTicketId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(t => t.DeletedByUser).WithMany()
            .HasForeignKey(t => t.DeletedByUserId).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.CreatedAt);
        builder.HasIndex(t => t.AssignedToUserId);
        builder.HasIndex(t => t.CreatedByUserId);
        builder.HasIndex(t => t.ProductId);
    }
}
