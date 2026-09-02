using GrandmastersHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GrandmastersHub.Infrastructure.Data.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(product => product.ProductId);
        builder.Property(product => product.Name).HasMaxLength(200).IsRequired();
        builder.Property(product => product.Description).HasMaxLength(2000);
        builder.Property(product => product.ImageUrl).HasMaxLength(2048);
        builder.Property(product => product.Price).HasPrecision(18, 2);
        builder.HasOne(product => product.Category)
            .WithMany(category => category.Products)
            .HasForeignKey(product => product.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(category => category.CategoryId);
        builder.Property(category => category.Name).HasMaxLength(100).IsRequired();
        builder.Property(category => category.Description).HasMaxLength(1000);
        builder.HasIndex(category => category.Name).IsUnique();
    }
}
