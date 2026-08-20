using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechnicalSupportService.Data.Entities;

namespace TechnicalSupportService.Data.Configurations;

public class TicketNumberCounterConfiguration : IEntityTypeConfiguration<TicketNumberCounter>
{
    public void Configure(EntityTypeBuilder<TicketNumberCounter> builder)
    {
        builder.HasKey(c => c.YearMonth);
        builder.Property(c => c.YearMonth).HasMaxLength(7);
        builder.Property(c => c.LastNumber).HasDefaultValue(0);
    }
}
