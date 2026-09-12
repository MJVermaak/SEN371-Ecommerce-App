using System.Net;
using System.Net.Http.Json;
using GrandmastersHub.Application.DTOs.Cart;
using GrandmastersHub.Application.DTOs.Orders;
using GrandmastersHub.Domain.Entities;
using GrandmastersHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GrandmastersHub.Tests;

public sealed class CheckoutFlowTests
{
    [Theory]
    [InlineData("GET", "/api/v1/orders/checkout")]
    [InlineData("POST", "/api/v1/orders")]
    [InlineData("GET", "/api/v1/orders")]
    [InlineData("GET", "/api/v1/orders/1")]
    [InlineData("GET", "/api/v1/orders/by-checkout/55d02185-e0c1-40dc-9f8b-d7b427408d62")]
    public async Task Orders_RequireAuthentication(string method, string path)
    {
        using var app = new CheckoutTestFactory();
        await app.InitializeAsync();
        using var client = app.Client();
        using var request = new HttpRequestMessage(new HttpMethod(method), path) { Content = JsonContent.Create(new {}) };
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.SendAsync(request)).StatusCode);
    }

    [Fact]
    public async Task Place_PersistsReceiptUnpaidPaymentStockAndHistory()
    {
        using var app = new CheckoutTestFactory();
        await app.InitializeAsync();
        using var client = app.Client(app.Alice);
        await app.Add(client);
        var request = await CheckoutTestFactory.Request(client);
        var response = await client.PostAsJsonAsync("/api/v1/orders", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var order = (await response.Content.ReadFromJsonAsync<OrderDetailsResponse>())!;
        Assert.Equal(251m, order.Subtotal);
        Assert.Equal(75m, order.DeliveryFee);
        Assert.Equal(326m, order.TotalAmount);
        Assert.Equal("Unpaid", order.PaymentStatus);
        Assert.Equal("Demo", order.PaymentMethod);
        Assert.Equal("Pending", order.Status);
        Assert.Equal("ZAR", order.Currency);
        Assert.Equal("Alice Test", order.ShippingAddress!.RecipientName);
        Assert.Equal(125.50m, Assert.Single(order.Items).UnitPrice);
        Assert.NotNull(response.Headers.Location);
        await app.AssertState(1, 3, 0);

        using var reloaded = app.Client(app.Alice);
        var detail = await reloaded.GetFromJsonAsync<OrderDetailsResponse>($"/api/v1/orders/{order.OrderId}");
        Assert.Equal(order.TotalAmount, detail!.TotalAmount);
        Assert.Empty((await reloaded.GetFromJsonAsync<CartDto>("/api/v1/cart"))!.Items);
        var history = await reloaded.GetFromJsonAsync<OrderPageResponse>("/api/v1/orders");
        Assert.Equal(order.OrderId, Assert.Single(history!.Items).OrderId);
        Assert.Equal(2, history.Items[0].TotalQuantity);
    }

    [Fact]
    public async Task Place_IgnoresClientPricesPaymentStatusAndUserId()
    {
        using var app = new CheckoutTestFactory();
        await app.InitializeAsync();
        using var client = app.Client(app.Alice);
        await app.Add(client);
        var request = await CheckoutTestFactory.Request(client);
        var response = await client.PostAsJsonAsync("/api/v1/orders", new
        {
            request.CheckoutKey, request.CartFingerprint, request.ShippingAddress,
            userId = app.Bob.UserId, totalAmount = 0.01m, deliveryFee = 0, paymentStatus = "Paid"
        });
        response.EnsureSuccessStatusCode();
        var order = (await response.Content.ReadFromJsonAsync<OrderDetailsResponse>())!;
        Assert.Equal(326m, order.TotalAmount);
        Assert.Equal("Unpaid", order.PaymentStatus);
        using var bob = app.Client(app.Bob);
        Assert.Empty((await bob.GetFromJsonAsync<OrderPageResponse>("/api/v1/orders"))!.Items);
    }

    [Fact]
    public async Task Place_ReplaysKeyWithoutTouchingANewCart()
    {
        using var app = new CheckoutTestFactory();
        await app.InitializeAsync();
        using var client = app.Client(app.Alice);
        await app.Add(client);
        var request = await CheckoutTestFactory.Request(client);
        var first = await ReadOrder(await client.PostAsJsonAsync("/api/v1/orders", request));
        await app.Add(client, 1);
        var replay = await client.PostAsJsonAsync("/api/v1/orders", request);
        Assert.Equal(HttpStatusCode.OK, replay.StatusCode);
        Assert.Equal(first.OrderId, (await ReadOrder(replay)).OrderId);
        var recovered = await client.GetFromJsonAsync<OrderDetailsResponse>($"/api/v1/orders/by-checkout/{request.CheckoutKey}");
        Assert.Equal(first.OrderId, recovered!.OrderId);
        await app.AssertState(1, 3, 1);
    }

    [Fact]
    public async Task Place_RejectsKeyReuseWithDifferentAddress()
    {
        using var app = new CheckoutTestFactory();
        await app.InitializeAsync();
        using var client = app.Client(app.Alice);
        await app.Add(client);
        var request = await CheckoutTestFactory.Request(client);
        await ReadOrder(await client.PostAsJsonAsync("/api/v1/orders", request));
        var changed = request with { ShippingAddress = request.ShippingAddress with { City = "Johannesburg" } };
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/v1/orders", changed)).StatusCode);
        await app.AssertState(1, 3, 0);
    }

    [Fact]
    public async Task Place_RejectsEmptyCart()
    {
        using var app = new CheckoutTestFactory();
        await app.InitializeAsync();
        using var client = app.Client(app.Alice);
        var request = await CheckoutTestFactory.Request(client);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/v1/orders", request)).StatusCode);
        await app.AssertState(0, 5, 0);
    }

    [Theory]
    [InlineData("key")]
    [InlineData("recipient")]
    [InlineData("postal")]
    [InlineData("country")]
    [InlineData("address")]
    [InlineData("fingerprint")]
    public async Task Place_RejectsInvalidInputWithoutChangingCart(string invalid)
    {
        using var app = new CheckoutTestFactory();
        await app.InitializeAsync();
        using var client = app.Client(app.Alice);
        await app.Add(client);
        var request = await CheckoutTestFactory.Request(client);
        request = invalid switch
        {
            "key" => request with { CheckoutKey = Guid.Empty },
            "recipient" => request with { ShippingAddress = request.ShippingAddress with { RecipientName = "   " } },
            "postal" => request with { ShippingAddress = request.ShippingAddress with { PostalCode = "invalid" } },
            "country" => request with { ShippingAddress = request.ShippingAddress with { Country = "Belgium" } },
            "address" => request with { ShippingAddress = null! },
            _ => request with { CartFingerprint = "invalid" }
        };
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/v1/orders", request)).StatusCode);
        await app.AssertState(0, 5, 2);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Place_RejectsStalePriceOrQuantity(bool changePrice)
    {
        using var app = new CheckoutTestFactory();
        await app.InitializeAsync();
        using var client = app.Client(app.Alice);
        await app.Add(client);
        var request = await CheckoutTestFactory.Request(client);
        if (changePrice) await app.Edit(async database =>
        {
            (await database.ProductVariants.FindAsync(app.Variant.ProductVariantId))!.Price = 200m;
            await database.SaveChangesAsync();
        });
        else await app.Add(client, 1);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/v1/orders", request)).StatusCode);
        await app.AssertState(0, 5, changePrice ? 2 : 3);
    }

    [Fact]
    public async Task Place_RechecksStockAfterPreview()
    {
        using var app = new CheckoutTestFactory();
        await app.InitializeAsync();
        using var client = app.Client(app.Alice);
        await app.Add(client);
        var request = await CheckoutTestFactory.Request(client);
        await app.Edit(async database =>
        {
            (await database.Inventory.SingleAsync(i => i.ProductVariantId == app.Variant.ProductVariantId)).Quantity = 1;
            await database.SaveChangesAsync();
        });
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/v1/orders", request)).StatusCode);
        await app.AssertState(0, 1, 2);
    }

    [Fact]
    public async Task Place_RollsBackStockAndCartIfOrderSaveFails()
    {
        using var app = new CheckoutTestFactory();
        await app.InitializeAsync();
        using var client = app.Client(app.Alice);
        await app.Add(client);
        var request = await CheckoutTestFactory.Request(client);
        app.SaveFailure.FailNextOrder = true;
        Assert.Equal(HttpStatusCode.InternalServerError, (await client.PostAsJsonAsync("/api/v1/orders", request)).StatusCode);
        await app.AssertState(0, 5, 2);
        await ReadOrder(await client.PostAsJsonAsync("/api/v1/orders", request));
        await app.AssertState(1, 3, 0);
    }

    [Fact]
    public async Task Receipt_DoesNotChangeWhenCatalogChanges()
    {
        using var app = new CheckoutTestFactory();
        await app.InitializeAsync();
        using var client = app.Client(app.Alice);
        await app.Add(client);
        var request = await CheckoutTestFactory.Request(client);
        var order = await ReadOrder(await client.PostAsJsonAsync("/api/v1/orders", request));
        await app.Edit(async database =>
        {
            (await database.Products.FindAsync(app.Product.ProductId))!.Name = "Changed product name";
            var variant = (await database.ProductVariants.FindAsync(app.Variant.ProductVariantId))!;
            variant.Price = 900m; variant.Name = "Changed option";
            await database.SaveChangesAsync();
        });
        var receipt = (await client.GetFromJsonAsync<OrderDetailsResponse>($"/api/v1/orders/{order.OrderId}"))!;
        Assert.Equal("Test tournament board", receipt.Items[0].ProductName);
        Assert.Equal("Walnut", receipt.Items[0].VariantName);
        Assert.Equal(125.50m, receipt.Items[0].UnitPrice);
        Assert.Equal(326m, receipt.TotalAmount);
        Assert.Equal("12 Test Street", receipt.ShippingAddress!.StreetAddress);
    }

    [Fact]
    public async Task History_IsPrivateAndPaginated()
    {
        using var app = new CheckoutTestFactory();
        await app.InitializeAsync();
        using var alice = app.Client(app.Alice);
        using var bob = app.Client(app.Bob);
        await app.Add(alice, 1);
        var firstRequest = await CheckoutTestFactory.Request(alice);
        var first = await ReadOrder(await alice.PostAsJsonAsync("/api/v1/orders", firstRequest));
        await app.Add(alice, 1);
        var second = await ReadOrder(await alice.PostAsJsonAsync("/api/v1/orders", await CheckoutTestFactory.Request(alice)));
        Assert.Equal(HttpStatusCode.NotFound, (await bob.GetAsync($"/api/v1/orders/{first.OrderId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await bob.GetAsync($"/api/v1/orders/by-checkout/{firstRequest.CheckoutKey}")).StatusCode);
        Assert.Empty((await bob.GetFromJsonAsync<OrderPageResponse>("/api/v1/orders"))!.Items);
        var page1 = (await alice.GetFromJsonAsync<OrderPageResponse>("/api/v1/orders?page=1&pageSize=1"))!;
        var page2 = (await alice.GetFromJsonAsync<OrderPageResponse>("/api/v1/orders?page=2&pageSize=1"))!;
        Assert.Equal(2, page1.TotalCount);
        Assert.Equal(second.OrderId, Assert.Single(page1.Items).OrderId);
        Assert.Equal(first.OrderId, Assert.Single(page2.Items).OrderId);
        Assert.Equal(HttpStatusCode.BadRequest, (await alice.GetAsync("/api/v1/orders?page=0")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await alice.GetAsync("/api/v1/orders?pageSize=51")).StatusCode);
    }

    [Fact]
    public async Task Orders_RejectDeletedAccountTokens()
    {
        using var app = new CheckoutTestFactory();
        await app.InitializeAsync();
        using var alice = app.Client(app.Alice);
        var request = await CheckoutTestFactory.Request(alice);
        await app.Edit(async database =>
        {
            database.Users.Remove((await database.Users.FindAsync(app.Alice.UserId))!);
            await database.SaveChangesAsync();
        });
        Assert.Equal(HttpStatusCode.Unauthorized, (await alice.GetAsync("/api/v1/orders")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await alice.GetAsync("/api/v1/orders/checkout")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await alice.PostAsJsonAsync("/api/v1/orders", request)).StatusCode);
    }

    [Fact]
    public void Migration_MetadataMatchesSqlServerModel()
    {
        using var database = new GrandmastersDbContext(new DbContextOptionsBuilder<GrandmastersDbContext>()
            .UseSqlServer("Server=localhost;Database=unused_metadata_check;Integrated Security=True;TrustServerCertificate=True").Options);
        Assert.False(database.Database.HasPendingModelChanges());
        Assert.Contains("20260912094500_CheckoutOrders", database.Database.GetMigrations());
        var sql = database.GetService<IMigrator>().GenerateScript("20260911215338_SeedDemoData", "20260912094500_CheckoutOrders");
        Assert.Contains("[ReceiptJson]", sql);
        Assert.Contains("IX_Orders_UserId_CheckoutKey", sql);
        Assert.DoesNotContain("DROP TABLE", sql, StringComparison.OrdinalIgnoreCase);
    }

    internal static async Task<OrderDetailsResponse> ReadOrder(HttpResponseMessage response)
    {
        Assert.True(response.IsSuccessStatusCode, $"{response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        return (await response.Content.ReadFromJsonAsync<OrderDetailsResponse>())!;
    }
}
