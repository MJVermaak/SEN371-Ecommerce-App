using GrandmastersHub.Domain.Entities;
using GrandmastersHub.Infrastructure.Data;

namespace GrandmastersHub.Tests;

public sealed class CommercePersistenceTests
{
    [Fact]
    public void IntegratedCommerceModel_UsesIntForeignKeysForUsersAndCompleteCommerceEntities()
    {
        Assert.Equal(typeof(int), typeof(User).GetProperty(nameof(User.UserId))!.PropertyType);
        Assert.Equal(typeof(int), typeof(Cart).GetProperty(nameof(Cart.UserId))!.PropertyType);
        Assert.Equal(typeof(int), typeof(Order).GetProperty(nameof(Order.UserId))!.PropertyType);
        Assert.Equal(typeof(int), typeof(Review).GetProperty(nameof(Review.UserId))!.PropertyType);
        Assert.Equal(typeof(int), typeof(ProductVariant).GetProperty(nameof(ProductVariant.ProductVariantId))!.PropertyType);
        Assert.Equal(typeof(int), typeof(Inventory).GetProperty(nameof(Inventory.ProductVariantId))!.PropertyType);
    }

}
