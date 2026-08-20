using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechnicalSupportService.Data.Entities;

namespace TechnicalSupportService.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.ProductType).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.CurrentVersion).HasMaxLength(20);
        builder.Property(p => p.Description).HasMaxLength(500);
    }
}
