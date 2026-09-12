using System.Net;
using System.Net.Http.Json;
using GrandmastersHub.Application.DTOs.Cart;
using Microsoft.EntityFrameworkCore;

namespace GrandmastersHub.Tests;

// Run only with CHECKOUT_SQLSERVER_CONNECTION. The factory always creates a unique disposable database.
public sealed class CheckoutConcurrencyTests
{
    [SqlServerFact]
    public async Task SameKey_ConcurrentSubmissionsCreateOneOrder()
    {
        using var app = new CheckoutTestFactory();
        await app.InitializeAsync();
        using var first = app.Client(app.Alice);
        using var second = app.Client(app.Alice);
        await app.Add(first);
        var request = await CheckoutTestFactory.Request(first);
        var responses = await Task.WhenAll(first.PostAsJsonAsync("/api/v1/orders", request),
            second.PostAsJsonAsync("/api/v1/orders", request));
        Assert.Single(responses, response => response.StatusCode == HttpStatusCode.Created);
        Assert.Single(responses, response => response.StatusCode == HttpStatusCode.OK);
        var orders = await Task.WhenAll(responses.Select(CheckoutFlowTests.ReadOrder));
        Assert.Equal(orders[0].OrderId, orders[1].OrderId);
        await app.AssertState(1, 3, 0);
    }

    [SqlServerFact]
    public async Task LastUnit_ConcurrentCustomersCannotOversell()
    {
        using var app = new CheckoutTestFactory();
        await app.InitializeAsync();
        await app.Edit(async database =>
        {
            (await database.Inventory.SingleAsync(i => i.ProductVariantId == app.Variant.ProductVariantId)).Quantity = 1;
            await database.SaveChangesAsync();
        });
        using var alice = app.Client(app.Alice);
        using var bob = app.Client(app.Bob);
        await app.Add(alice, 1); await app.Add(bob, 1);
        var aliceRequest = await CheckoutTestFactory.Request(alice);
        var bobRequest = await CheckoutTestFactory.Request(bob);
        var responses = await Task.WhenAll(alice.PostAsJsonAsync("/api/v1/orders", aliceRequest),
            bob.PostAsJsonAsync("/api/v1/orders", bobRequest));
        Assert.Single(responses, response => response.StatusCode == HttpStatusCode.Created);
        Assert.Single(responses, response => response.StatusCode == HttpStatusCode.Conflict);
        await app.AssertState(1, 0, 1);
        var remaining = responses[0].StatusCode == HttpStatusCode.Conflict ? alice : bob;
        Assert.Single((await remaining.GetFromJsonAsync<CartDto>("/api/v1/cart"))!.Items);
    }
}

public sealed class SqlServerFactAttribute : FactAttribute
{
    public SqlServerFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("CHECKOUT_SQLSERVER_CONNECTION")))
            Skip = "Requires the disposable SQL Server test service.";
    }
}
