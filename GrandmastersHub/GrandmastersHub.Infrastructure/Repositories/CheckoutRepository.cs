using System.Data;
using GrandmastersHub.Application.Exceptions;
using GrandmastersHub.Application.Interfaces;
using GrandmastersHub.Domain.Entities;
using GrandmastersHub.Domain.Interfaces;
using GrandmastersHub.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GrandmastersHub.Infrastructure.Repositories;

public sealed class CheckoutRepository(GrandmastersDbContext context, ICartRepository carts) : ICheckoutRepository
{
    private IQueryable<Order> ReadOrders(int userId) => context.Orders.AsNoTracking().AsSplitQuery()
        .Where(order => order.UserId == userId).Include(order => order.Payment)
        .Include(order => order.Items).ThenInclude(item => item.ProductVariant)!
            .ThenInclude(variant => variant!.Product);

    public Task<Order?> GetAsync(int userId, int orderId, CancellationToken cancellationToken) =>
        ReadOrders(userId).SingleOrDefaultAsync(order => order.OrderId == orderId, cancellationToken);

    public Task<Order?> GetByKeyAsync(int userId, string checkoutKey, CancellationToken cancellationToken) =>
        ReadOrders(userId).SingleOrDefaultAsync(order => order.CheckoutKey == checkoutKey, cancellationToken);

    public async Task<OrderPage> ListAsync(int userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = ReadOrders(userId);
        var count = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(order => order.CreatedAt).ThenByDescending(order => order.OrderId)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new OrderPage(items, count);
    }

    public async Task<CheckoutWriteResult> PlaceAsync(int userId, string checkoutKey,
        Func<Cart, Order> createOrder, CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        try
        {
            // Use the same account lock as cart mutations, including before a cart exists.
            var user = context.Database.IsSqlServer()
                ? await context.Users.FromSqlInterpolated(
                    $"SELECT * FROM [Users] WITH (UPDLOCK, HOLDLOCK) WHERE [UserId] = {userId}")
                    .SingleOrDefaultAsync(cancellationToken)
                : await context.Users.SingleOrDefaultAsync(user => user.UserId == userId, cancellationToken);
            if (user is null)
                throw new OrderRequestException(OrderFailure.Unauthenticated, "Please sign in again.");

            var existing = await GetByKeyAsync(userId, checkoutKey, cancellationToken);
            if (existing is not null)
            {
                await transaction.CommitAsync(cancellationToken);
                return new CheckoutWriteResult(existing, false);
            }
            var cart = await carts.GetByUserIdAsync(userId)
                ?? throw new OrderRequestException(OrderFailure.InvalidRequest, "Your cart is empty.");
            var order = createOrder(cart);

            // Conditional updates prevent overselling across customers. Every update is in
            // this transaction; any failure rolls back stock, payment, order and cart changes.
            foreach (var item in order.Items.OrderBy(item => item.ProductVariantId))
            {
                var changed = await context.Inventory
                    .Where(stock => stock.ProductVariantId == item.ProductVariantId && stock.Quantity >= item.Quantity)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(stock => stock.Quantity, stock => stock.Quantity - item.Quantity)
                        .SetProperty(stock => stock.UpdatedAt, DateTime.UtcNow), cancellationToken);
                if (changed != 1)
                    throw new OrderRequestException(OrderFailure.Conflict,
                        "Stock changed during checkout. Your order was not placed. Please review your cart.");
            }
            context.Orders.Add(order);
            context.CartItems.RemoveRange(cart.Items);
            cart.UpdatedAt = DateTime.UtcNow;
            // ExecuteUpdate does not change tracked Inventory objects. Leave them untouched.
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new CheckoutWriteResult(order, true);
        }
        catch (Exception exception) when (exception.GetBaseException() is SqlException sql
            && sql.Number is 1205 or 2601 or 2627)
        {
            throw new OrderRequestException(OrderFailure.Conflict,
                "Another request changed checkout. Check your order history, then retry if no order was placed.");
        }
    }
}
