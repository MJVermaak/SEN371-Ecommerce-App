using GrandmastersHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GrandmastersHub.Infrastructure.Data.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.HasKey(item => item.CartItemId);
        
        // Ensure item.CartId exists in CartItem.cs
        builder.HasOne(item => item.Cart)
            .WithMany(cart => cart.Items) // Changed from cart.CartItems to match your Cart entity
            .HasForeignKey(item => item.CartId);

        // NOTE: If your CartItem points to a ProductVariant instead of Product, 
        // change "Product" and "ProductId" to match the actual property names in CartItem.cs.
    }
}