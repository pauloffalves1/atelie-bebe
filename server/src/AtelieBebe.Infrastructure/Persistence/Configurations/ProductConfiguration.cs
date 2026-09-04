using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtelieBebe.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Slug).IsRequired().HasMaxLength(220);
        builder.HasIndex(p => p.Slug).IsUnique();
        builder.Property(p => p.Description).HasMaxLength(2000);
        builder.Property(p => p.Category).IsRequired().HasMaxLength(100);
        builder.HasIndex(p => p.Category);
        builder.Property(p => p.ImageUrl).HasMaxLength(500);

        builder.Property(p => p.Price)
            .HasConversion(money => money.Amount, amount => Money.FromReais(amount))
            .HasColumnName("PriceAmount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();
    }
}
