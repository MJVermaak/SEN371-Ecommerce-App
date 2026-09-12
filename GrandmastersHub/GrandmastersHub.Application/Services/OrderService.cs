using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using GrandmastersHub.Application.DTOs.Orders;
using GrandmastersHub.Application.Exceptions;
using GrandmastersHub.Application.Interfaces;
using GrandmastersHub.Domain.Entities;
using GrandmastersHub.Domain.Interfaces;

namespace GrandmastersHub.Application.Services;

public sealed class OrderService(ICheckoutRepository orders, ICartRepository carts, IUserRepository users)
    : IOrderService
{
    // Demonstration delivery rate, not a courier quotation. No payment is collected.
    public const decimal DemoDeliveryFee = 75m;

    public async Task<CheckoutPreview> PreviewAsync(int userId, CancellationToken cancellationToken)
    {
        await RequireUser(userId, cancellationToken);
        return Preview(await carts.GetByUserIdAsync(userId));
    }

    public async Task<PlaceOrderResult> PlaceAsync(int userId, PlaceOrderRequest request,
        CancellationToken cancellationToken)
    {
        var address = Validate(request);
        var requestHash = Hash(JsonSerializer.Serialize(new
        {
            CartFingerprint = request.CartFingerprint.ToUpperInvariant(), ShippingAddress = address
        }));
        var key = request.CheckoutKey.ToString("D");
        var result = await orders.PlaceAsync(userId, key, cart =>
        {
            if (cart.Items.Count == 0)
                throw new OrderRequestException(OrderFailure.InvalidRequest, "Your cart is empty.");
            var quote = Preview(cart);
            if (!string.Equals(request.CartFingerprint, quote.CartFingerprint, StringComparison.OrdinalIgnoreCase))
                throw new OrderRequestException(OrderFailure.Conflict,
                    "Your cart or its prices changed. Review the updated checkout before placing your order.");
            if (quote.Items.Any(item => item.Quantity is < 1 or > 99 || item.UnitPrice <= 0))
                throw new OrderRequestException(OrderFailure.InvalidRequest, "Your cart contains an invalid item. Update your cart first.");
            if (quote.Items.Any(item => !item.IsAvailable))
                throw new OrderRequestException(OrderFailure.Conflict,
                    "An item no longer has enough stock. Update your cart before placing your order.");

            var lines = quote.Items.Select(item => new OrderLineSnapshot(item.ProductId,
                item.ProductVariantId, item.ProductName, item.VariantName, item.ImageUrl,
                item.Quantity, item.UnitPrice)).ToList();
            return new Order
            {
                UserId = userId, CheckoutKey = key, Status = "Pending", TotalAmount = quote.TotalAmount,
                ReceiptJson = JsonSerializer.Serialize(new CheckoutReceipt(1, requestHash, address,
                    lines, quote.Subtotal, quote.DeliveryFee, quote.Currency)),
                Items = lines.Select(item => new OrderItem
                {
                    ProductVariantId = item.ProductVariantId, Quantity = item.Quantity, UnitPrice = item.UnitPrice
                }).ToList(),
                Payment = new Payment { Status = "Unpaid", PaymentMethod = "Demo", Amount = quote.TotalAmount }
            };
        }, cancellationToken);

        // A retry may replay the original request, but cannot repurpose its key.
        var receipt = ReadReceipt(result.Order);
        if (receipt?.RequestFingerprint != requestHash)
            throw new OrderRequestException(OrderFailure.Conflict,
                "This checkout reference already belongs to a different submission. Check your order history.");
        return new PlaceOrderResult(Details(result.Order), result.Created);
    }

    public async Task<OrderDetailsResponse> GetAsync(int userId, int orderId, CancellationToken cancellationToken)
    {
        await RequireUser(userId, cancellationToken);
        return Details(await orders.GetAsync(userId, orderId, cancellationToken)
            ?? throw new OrderRequestException(OrderFailure.NotFound, "Order not found."));
    }

    public async Task<OrderDetailsResponse> GetByKeyAsync(int userId, Guid key, CancellationToken cancellationToken)
    {
        await RequireUser(userId, cancellationToken);
        return Details(await orders.GetByKeyAsync(userId, key.ToString("D"), cancellationToken)
            ?? throw new OrderRequestException(OrderFailure.NotFound, "No order has been recorded for this checkout reference."));
    }

    public async Task<OrderPageResponse> ListAsync(int userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        await RequireUser(userId, cancellationToken);
        if (page is < 1 or > 100000 || pageSize is < 1 or > 50)
            throw new OrderRequestException(OrderFailure.InvalidRequest, "Page must be positive and page size must be between 1 and 50.");
        var result = await orders.ListAsync(userId, page, pageSize, cancellationToken);
        var summaries = result.Items.Select(order =>
        {
            var details = Details(order);
            return new OrderSummaryResponse(order.OrderId, details.OrderNumber, order.CreatedAt,
                order.Status, details.PaymentStatus, order.TotalAmount, details.Currency,
                details.Items.Sum(item => item.Quantity));
        }).ToList();
        return new OrderPageResponse(summaries, page, pageSize, result.TotalCount);
    }

    private async Task RequireUser(int userId, CancellationToken cancellationToken)
    {
        if (userId <= 0 || await users.GetByIdAsync(userId, cancellationToken) is null)
            throw new OrderRequestException(OrderFailure.Unauthenticated, "Please sign in again.");
    }

    private static DeliveryAddress Validate(PlaceOrderRequest request)
    {
        if (request.CheckoutKey == Guid.Empty || request.ShippingAddress is null)
            throw new OrderRequestException(OrderFailure.InvalidRequest, "A checkout reference and delivery address are required.");
        var errors = new List<ValidationResult>();
        if (!Validator.TryValidateObject(request, new ValidationContext(request), errors, true)
            || !Validator.TryValidateObject(request.ShippingAddress,
                new ValidationContext(request.ShippingAddress), errors, true))
            throw new OrderRequestException(OrderFailure.InvalidRequest, errors[0].ErrorMessage ?? "Check your delivery details.");
        var address = request.ShippingAddress;
        if (!string.Equals(address.Country.Trim(), "South Africa", StringComparison.OrdinalIgnoreCase))
            throw new OrderRequestException(OrderFailure.InvalidRequest, "Demo delivery is available in South Africa only.");
        return address with
        {
            RecipientName = address.RecipientName.Trim(), StreetAddress = address.StreetAddress.Trim(),
            City = address.City.Trim(), Province = address.Province.Trim(), PostalCode = address.PostalCode.Trim(),
            Country = "South Africa"
        };
    }

    private static CheckoutPreview Preview(Cart? cart)
    {
        var lines = (cart?.Items ?? []).OrderBy(item => item.CartItemId).Select(item =>
        {
            var variant = item.ProductVariant
                ?? throw new OrderRequestException(OrderFailure.Conflict, "A cart option is no longer available.");
            var product = variant.Product
                ?? throw new OrderRequestException(OrderFailure.Conflict, "A cart product is no longer available.");
            return new CheckoutLine(item.CartItemId, product.ProductId, variant.ProductVariantId,
                product.Name, variant.Name, product.Images.OrderBy(image => image.ProductImageId).FirstOrDefault()?.ImageUrl,
                item.Quantity, variant.Price, Math.Max(0, variant.Inventory?.Quantity ?? 0));
        }).ToList();
        var subtotal = lines.Sum(item => item.LineTotal);
        var delivery = lines.Count == 0 ? 0m : DemoDeliveryFee;
        var fingerprint = Hash(JsonSerializer.Serialize(new
        {
            DeliveryFee = delivery,
            Items = lines.Select(item => new { item.CartItemId, item.ProductVariantId, item.Quantity, item.UnitPrice })
        }));
        return new CheckoutPreview(lines, fingerprint, subtotal, delivery, subtotal + delivery, "ZAR");
    }

    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    private static CheckoutReceipt? ReadReceipt(Order order) => order.ReceiptJson is null
        ? null : JsonSerializer.Deserialize<CheckoutReceipt>(order.ReceiptJson);

    private static OrderDetailsResponse Details(Order order)
    {
        var receipt = ReadReceipt(order);
        // Legacy orders did not store names or addresses. Never invent a delivery address.
        var items = receipt?.Items ?? order.Items.OrderBy(item => item.OrderItemId).Select(item =>
            new OrderLineSnapshot(item.ProductVariant?.ProductId ?? 0, item.ProductVariantId,
                item.ProductVariant?.Product?.Name ?? "Product unavailable",
                item.ProductVariant?.Name ?? "Option unavailable", null, item.Quantity, item.UnitPrice)).ToList();
        var subtotal = receipt?.Subtotal ?? items.Sum(item => item.LineTotal);
        return new OrderDetailsResponse(order.OrderId, $"GMH-{order.OrderId:D6}", order.CreatedAt,
            order.Status, order.Payment?.Status ?? "Unpaid", order.Payment?.PaymentMethod ?? "Not recorded",
            receipt?.Currency ?? "ZAR", subtotal, receipt?.DeliveryFee ?? 0m, order.TotalAmount,
            receipt?.ShippingAddress, items);
    }
}
