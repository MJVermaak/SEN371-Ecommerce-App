using GrandmastersHub.Application.DTOs.Orders;

namespace GrandmastersHub.Application.Interfaces;

public interface IOrderService
{
    Task<CheckoutPreview> PreviewAsync(int userId, CancellationToken cancellationToken);
    Task<PlaceOrderResult> PlaceAsync(int userId, PlaceOrderRequest request, CancellationToken cancellationToken);
    Task<OrderDetailsResponse> GetAsync(int userId, int orderId, CancellationToken cancellationToken);
    Task<OrderDetailsResponse> GetByKeyAsync(int userId, Guid key, CancellationToken cancellationToken);
    Task<OrderPageResponse> ListAsync(int userId, int page, int pageSize, CancellationToken cancellationToken);
}
