using AtelieBebe.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtelieBebe.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Type).IsRequired().HasMaxLength(400);
        builder.Property(m => m.Content).IsRequired().HasColumnType("TEXT");
        builder.Property(m => m.Error).HasColumnType("TEXT");

        builder.HasIndex(m => m.ProcessedOn);
    }
}
