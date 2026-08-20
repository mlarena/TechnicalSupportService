using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechnicalSupportService.Data.Entities;

namespace TechnicalSupportService.Data.Configurations;

public class TicketHistoryConfiguration : IEntityTypeConfiguration<TicketHistory>
{
    public void Configure(EntityTypeBuilder<TicketHistory> builder)
    {
        builder.HasKey(h => h.Id);
        builder.Property(h => h.FieldName).HasMaxLength(50);
        builder.Property(h => h.ChangeType).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(h => h.Ticket).WithMany(t => t.History)
            .HasForeignKey(h => h.TicketId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(h => h.ChangedByUser).WithMany()
            .HasForeignKey(h => h.ChangedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(h => h.Comment).WithMany()
            .HasForeignKey(h => h.CommentId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(h => h.Attachment).WithMany()
            .HasForeignKey(h => h.AttachmentId).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(h => new { h.TicketId, h.ChangedAt });
    }
}
