using GrandmastersHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GrandmastersHub.Infrastructure.Data.Configurations;

public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> builder)
    {
        builder.HasKey(i => i.InventoryId);

        builder.Property(i => i.Quantity)
            .IsRequired();

        builder.HasIndex(i => i.ProductVariantId)
            .IsUnique();
    }
}