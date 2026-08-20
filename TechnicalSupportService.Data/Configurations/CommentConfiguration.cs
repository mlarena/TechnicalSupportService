using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechnicalSupportService.Data.Entities;

namespace TechnicalSupportService.Data.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Content).IsRequired();
        builder.HasOne(c => c.Ticket).WithMany(t => t.Comments)
            .HasForeignKey(c => c.TicketId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(c => c.AuthorUser).WithMany()
            .HasForeignKey(c => c.AuthorUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(c => c.TicketId);
    }
}
