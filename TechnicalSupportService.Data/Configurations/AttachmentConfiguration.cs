using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechnicalSupportService.Data.Entities;

namespace TechnicalSupportService.Data.Configurations;

public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.FileName).IsRequired().HasMaxLength(255);
        builder.Property(a => a.StoredFileName).IsRequired().HasMaxLength(255);
        builder.Property(a => a.FilePath).IsRequired().HasMaxLength(500);
        builder.Property(a => a.MimeType).IsRequired().HasMaxLength(100);
        builder.HasOne(a => a.Ticket).WithMany(t => t.Attachments)
            .HasForeignKey(a => a.TicketId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(a => a.UploadedByUser).WithMany()
            .HasForeignKey(a => a.UploadedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(a => a.TicketId);
    }
}
