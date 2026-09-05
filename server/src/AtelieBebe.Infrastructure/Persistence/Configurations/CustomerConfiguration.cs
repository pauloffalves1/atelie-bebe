using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtelieBebe.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.PasswordHash).IsRequired();
        builder.Property(c => c.Phone).HasMaxLength(30);

        builder.Property(c => c.Email)
            .HasConversion(email => email.Value, value => Email.Create(value))
            .HasColumnName("Email")
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(c => c.Cpf)
            .HasConversion(cpf => cpf == null ? null : cpf.Value, value => value == null ? null : Cpf.Create(value))
            .HasColumnName("Cpf")
            .HasMaxLength(11);

        builder.HasIndex(c => c.Email).IsUnique();
        builder.HasIndex(c => c.Cpf).IsUnique();
    }
}
