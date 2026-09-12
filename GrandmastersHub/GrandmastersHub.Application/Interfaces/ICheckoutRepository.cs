using GrandmastersHub.Domain.Entities;

namespace GrandmastersHub.Application.Interfaces;

public sealed record CheckoutWriteResult(Order Order, bool Created);
public sealed record OrderPage(IReadOnlyList<Order> Items, int TotalCount);

public interface ICheckoutRepository
{
    Task<Order?> GetAsync(int userId, int orderId, CancellationToken cancellationToken);
    Task<Order?> GetByKeyAsync(int userId, string checkoutKey, CancellationToken cancellationToken);
    Task<OrderPage> ListAsync(int userId, int page, int pageSize, CancellationToken cancellationToken);
    // The callback runs under the account/cart transaction. Existing keys bypass it.
    Task<CheckoutWriteResult> PlaceAsync(int userId, string checkoutKey,
        Func<Cart, Order> createOrder, CancellationToken cancellationToken);
}
