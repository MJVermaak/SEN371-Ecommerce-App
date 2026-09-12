using System.Data;
using GrandmastersHub.Application.Exceptions;
using GrandmastersHub.Domain.Entities;
using GrandmastersHub.Domain.Interfaces;
using GrandmastersHub.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GrandmastersHub.Infrastructure.Repositories;

public class CartRepository(GrandmastersDbContext context) : ICartRepository
{
    private IQueryable<Cart> WithItems() => context.Carts
        .AsSplitQuery()
        .Include(cart => cart.Items).ThenInclude(item => item.ProductVariant)!
            .ThenInclude(variant => variant!.Inventory)
        .Include(cart => cart.Items).ThenInclude(item => item.ProductVariant)!
            .ThenInclude(variant => variant!.Product)!.ThenInclude(product => product!.Images)
        .Include(cart => cart.Items).ThenInclude(item => item.ProductVariant)!
            .ThenInclude(variant => variant!.Product)!.ThenInclude(product => product!.Category);

    public Task<Cart?> GetByIdAsync(int id) =>
        WithItems().FirstOrDefaultAsync(cart => cart.CartId == id);

    public Task<Cart?> GetByUserIdAsync(int userId) =>
        WithItems().FirstOrDefaultAsync(cart => cart.UserId == userId);

    public async Task<Cart> AddAsync(Cart cart)
    {
        context.Carts.Add(cart);
        await context.SaveChangesAsync();
        return cart;
    }

    public async Task UpdateAsync(Cart cart)
    {
        context.Carts.Update(cart);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var cart = await context.Carts.FindAsync(id);
        if (cart is null) return;
        context.Carts.Remove(cart);
        await context.SaveChangesAsync();
    }

    public async Task<Cart> MutateAsync(int userId, Func<Cart, Task> mutation)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            // Lock the account, including when its cart does not exist yet. The SQL Server
            // lock serializes writes across requests and API instances, not just this process.
            var user = context.Database.IsSqlServer()
                ? await context.Users.FromSqlInterpolated(
                    $"SELECT * FROM [Users] WITH (UPDLOCK, HOLDLOCK) WHERE [UserId] = {userId}")
                    .SingleOrDefaultAsync()
                : await context.Users.SingleOrDefaultAsync(user => user.UserId == userId);
            if (user is null)
                throw new CartRequestException(CartError.Unauthenticated, "Please sign in again.");

            var cart = await GetByUserIdAsync(userId);
            if (cart is null)
            {
                cart = new Cart { UserId = userId };
                context.Carts.Add(cart);
            }

            var previousItems = cart.Items.ToList();
            await mutation(cart);
            context.CartItems.RemoveRange(previousItems.Where(item => !cart.Items.Contains(item)));
            cart.UpdatedAt = DateTime.UtcNow;
            // Save tracked changes only. Do not mark the product/inventory graph as modified.
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return cart;
        }
        catch (Exception exception) when (exception.GetBaseException() is SqlException sql
            && sql.Number is 1205 or 2601 or 2627)
        {
            throw new CartRequestException(CartError.Conflict,
                "Your cart changed in another request. Refresh it and try again.");
        }
    }
}
