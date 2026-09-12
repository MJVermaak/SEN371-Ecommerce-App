using System.ComponentModel.DataAnnotations;

namespace GrandmastersHub.Application.DTOs.Orders;

public sealed record DeliveryAddress
{
    [Required, StringLength(100)] public string RecipientName { get; init; } = string.Empty;
    [Required, StringLength(100)] public string StreetAddress { get; init; } = string.Empty;
    [Required, StringLength(100)] public string City { get; init; } = string.Empty;
    [Required, StringLength(100)] public string Province { get; init; } = string.Empty;
    [Required, RegularExpression(@"^\d{4}$")] public string PostalCode { get; init; } = string.Empty;
    [Required, StringLength(100)] public string Country { get; init; } = "South Africa";
}

public sealed record PlaceOrderRequest
{
    public Guid CheckoutKey { get; init; }
    [Required, RegularExpression("^[a-fA-F0-9]{64}$")]
    public string CartFingerprint { get; init; } = string.Empty;
    [Required] public DeliveryAddress ShippingAddress { get; init; } = new();
}

public sealed record CheckoutLine(int CartItemId, int ProductId, int ProductVariantId,
    string ProductName, string VariantName, string? ImageUrl, int Quantity,
    decimal UnitPrice, int StockQuantity)
{
    public decimal LineTotal => UnitPrice * Quantity;
    public bool IsAvailable => Quantity > 0 && Quantity <= StockQuantity && UnitPrice > 0;
}

public sealed record CheckoutPreview(IReadOnlyList<CheckoutLine> Items, string CartFingerprint,
    decimal Subtotal, decimal DeliveryFee, decimal TotalAmount, string Currency);

public sealed record OrderLineSnapshot(int ProductId, int ProductVariantId, string ProductName,
    string VariantName, string? ImageUrl, int Quantity, decimal UnitPrice)
{
    public decimal LineTotal => UnitPrice * Quantity;
}

public sealed record CheckoutReceipt(int Version, string RequestFingerprint,
    DeliveryAddress ShippingAddress, IReadOnlyList<OrderLineSnapshot> Items,
    decimal Subtotal, decimal DeliveryFee, string Currency);

public sealed record OrderDetailsResponse(int OrderId, string OrderNumber, DateTime CreatedAt,
    string Status, string PaymentStatus, string PaymentMethod, string Currency,
    decimal Subtotal, decimal DeliveryFee, decimal TotalAmount,
    DeliveryAddress? ShippingAddress, IReadOnlyList<OrderLineSnapshot> Items);

public sealed record OrderSummaryResponse(int OrderId, string OrderNumber, DateTime CreatedAt,
    string Status, string PaymentStatus, decimal TotalAmount, string Currency, int TotalQuantity);

public sealed record OrderPageResponse(IReadOnlyList<OrderSummaryResponse> Items,
    int Page, int PageSize, int TotalCount);

public sealed record PlaceOrderResult(OrderDetailsResponse Order, bool Created);
