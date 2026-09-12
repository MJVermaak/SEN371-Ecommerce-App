using GrandmastersHub.Application.DTOs.Cart;
using GrandmastersHub.Application.Exceptions;
using GrandmastersHub.Application.Interfaces;
using GrandmastersHub.Domain.Entities;
using GrandmastersHub.Domain.Interfaces;

namespace GrandmastersHub.Application.Services;

public sealed class CartService(ICartRepository carts, IProductRepository products, IUserRepository users)
    : ICartService
{
    public async Task<CartDto> GetAsync(int userId)
    {
        if (await users.GetByIdAsync(userId) is null)
            throw new CartRequestException(CartError.Unauthenticated, "Please sign in again.");
        return ToDto(await carts.GetByUserIdAsync(userId));
    }

    public async Task<CartDto> AddAsync(int userId, AddCartItemRequest request)
    {
        ValidateQuantity(request.Quantity);
        var cart = await carts.MutateAsync(userId, async cart =>
        {
            var product = await products.GetByIdAsync(request.ProductId)
                ?? throw new CartRequestException(CartError.NotFound, "This product no longer exists.");
            var variant = product.Variants.FirstOrDefault(v => v.ProductVariantId == request.ProductVariantId)
                ?? throw new CartRequestException(CartError.NotFound, "This option is not available for this product.");
            var item = cart.Items.FirstOrDefault(item => item.ProductVariantId == variant.ProductVariantId);
            var quantity = (long)(item?.Quantity ?? 0) + request.Quantity;
            if (quantity > 99)
                throw new CartRequestException(CartError.InvalidRequest, "A cart item cannot exceed 99 units.");
            ValidateStock(variant, (int)quantity);
            if (item is null)
            {
                cart.Items.Add(new CartItem
                {
                    ProductVariantId = variant.ProductVariantId,
                    ProductVariant = variant,
                    Quantity = (int)quantity
                });
            }
            else item.Quantity = (int)quantity;
        });
        return ToDto(cart);
    }

    public async Task<CartDto> UpdateAsync(int userId, int itemId, int quantity)
    {
        ValidateQuantity(quantity);
        var cart = await carts.MutateAsync(userId, cart =>
        {
            var item = FindItem(cart, itemId);
            var variant = item.ProductVariant
                ?? throw new CartRequestException(CartError.NotFound, "This product option no longer exists.");
            ValidateStock(variant, quantity);
            item.Quantity = quantity;
            return Task.CompletedTask;
        });
        return ToDto(cart);
    }

    public async Task<CartDto> RemoveAsync(int userId, int itemId)
    {
        var cart = await carts.MutateAsync(userId, cart =>
        {
            cart.Items.Remove(FindItem(cart, itemId));
            return Task.CompletedTask;
        });
        return ToDto(cart);
    }

    private static CartItem FindItem(Cart cart, int itemId) =>
        cart.Items.FirstOrDefault(item => item.CartItemId == itemId)
        ?? throw new CartRequestException(CartError.NotFound, "This item is not in your cart.");

    private static void ValidateQuantity(int quantity)
    {
        if (quantity is < 1 or > 99)
            throw new CartRequestException(CartError.InvalidRequest, "Quantity must be between 1 and 99.");
    }

    private static void ValidateStock(ProductVariant variant, int quantity)
    {
        var stock = Math.Max(0, variant.Inventory?.Quantity ?? 0);
        if (quantity > stock)
            throw new CartRequestException(CartError.Conflict,
                stock == 0 ? "This option is out of stock." : $"Only {stock} units of this option are available.");
    }

    private static CartDto ToDto(Cart? cart) => new(cart?.Items.OrderBy(item => item.CartItemId)
        .Select(item =>
        {
            var variant = item.ProductVariant!;
            var product = variant.Product!;
            return new CartItemDto(item.CartItemId, product.ProductId, variant.ProductVariantId,
                product.Name, variant.Name, product.Category?.Name ?? string.Empty,
                product.Images.OrderBy(image => image.ProductImageId).FirstOrDefault()?.ImageUrl,
                item.Quantity, variant.Price, Math.Max(0, variant.Inventory?.Quantity ?? 0));
        }).ToList() ?? []);
}
