using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.Enums;
using AtelieBebe.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtelieBebe.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.CustomerName).IsRequired().HasMaxLength(200);
        builder.Property(o => o.CustomerPhone).HasMaxLength(30);
        builder.Property(o => o.Notes).HasMaxLength(2000);
        builder.Property(o => o.CustomDetailsJson).HasColumnType("TEXT");
        builder.Property(o => o.ShippingAddressJson).HasColumnType("TEXT");

        builder.Property(o => o.Type).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.CustomerId);

        builder.Property(o => o.CustomerEmail)
            .HasConversion(email => email.Value, value => Email.Create(value))
            .HasColumnName("CustomerEmail")
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(o => o.CustomerCpf)
            .HasConversion(cpf => cpf == null ? null : cpf.Value, value => value == null ? null : Cpf.Create(value))
            .HasColumnName("CustomerCpf")
            .HasMaxLength(11);

        // Items are a child collection of the Order aggregate, only ever mutated through
        // Order's own methods (AddItem). EF is pointed at the private backing field so no
        // public setter is ever exposed to callers outside the aggregate.
        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(Order.Items))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(o => o.Total);
    }
}
