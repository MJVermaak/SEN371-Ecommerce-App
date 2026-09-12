using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GrandmastersHub.Application.DTOs.Cart;
using GrandmastersHub.Application.DTOs.Catalog;
using GrandmastersHub.Application.Interfaces;
using GrandmastersHub.Domain.Entities;
using GrandmastersHub.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace GrandmastersHub.Tests;

// Exercise the HTTP pipeline, JWT validation, services and EF repositories.
// Every test uses its own SQLite database, never the developer's SQL Server.
public sealed class ShoppingFlowTests
{
    [Fact]
    public async Task ProductDetails_ReturnImagesAndPricedStockedVariants()
    {
        using var app = new ShoppingFactory();
        await app.InitializeAsync();
        using var client = app.Client();
        var product = await client.GetFromJsonAsync<ProductDto>($"/api/v1/products/{app.Product.ProductId}");
        Assert.NotNull(product);
        Assert.Equal("Tournament board", product.Name);
        Assert.Single(product.ImageUrls);
        Assert.Equal(2, product.Variants.Count);
        Assert.Contains(product.Variants, v => v.ProductVariantId == app.InStock.ProductVariantId
            && v.Price == 125.50m && v.StockQuantity == 5);
        Assert.Contains(product.Variants, v => v.ProductVariantId == app.SoldOut.ProductVariantId && v.StockQuantity == 0);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/v1/products/999999")).StatusCode);
    }

    [Theory]
    [InlineData("GET", "/api/v1/cart")]
    [InlineData("POST", "/api/v1/cart/items")]
    [InlineData("PUT", "/api/v1/cart/items/1")]
    [InlineData("DELETE", "/api/v1/cart/items/1")]
    public async Task Cart_RequiresAuthentication(string method, string path)
    {
        using var app = new ShoppingFactory();
        await app.InitializeAsync();
        using var client = app.Client();
        using var request = new HttpRequestMessage(new HttpMethod(method), path)
        {
            Content = JsonContent.Create(new { productId = app.Product.ProductId,
                productVariantId = app.InStock.ProductVariantId, quantity = 1 })
        };
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.SendAsync(request)).StatusCode);
    }

    [Fact]
    public async Task Cart_AddMergeUpdateRemove_PersistsAcrossHttpClients()
    {
        using var app = new ShoppingFactory();
        await app.InitializeAsync();
        using var first = app.Client(app.Alice);
        Assert.Empty((await first.GetFromJsonAsync<CartDto>("/api/v1/cart"))!.Items);
        var cart = await ReadCart(await first.PostAsJsonAsync("/api/v1/cart/items", new
        {
            productId = app.Product.ProductId, productVariantId = app.InStock.ProductVariantId,
            quantity = 2, userId = app.Bob.UserId, unitPrice = 0.01m
        }));
        var item = Assert.Single(cart.Items);
        Assert.Equal(125.50m, item.UnitPrice);
        Assert.Equal(251m, cart.Subtotal);
        Assert.Equal(2, cart.TotalQuantity);
        Assert.NotNull(item.ImageUrl);

        using var reloaded = app.Client(app.Alice);
        Assert.Equal(cart.Subtotal, (await reloaded.GetFromJsonAsync<CartDto>("/api/v1/cart"))!.Subtotal);
        cart = await ReadCart(await app.Add(reloaded, 1));
        Assert.Single(cart.Items);
        Assert.Equal(3, cart.TotalQuantity);
        cart = await ReadCart(await reloaded.PutAsJsonAsync($"/api/v1/cart/items/{item.CartItemId}", new { quantity = 1 }));
        Assert.Equal(125.50m, cart.Subtotal);
        cart = await ReadCart(await reloaded.DeleteAsync($"/api/v1/cart/items/{item.CartItemId}"));
        Assert.Empty(cart.Items);
        Assert.Equal(0m, cart.Subtotal);
        Assert.Empty((await first.GetFromJsonAsync<CartDto>("/api/v1/cart"))!.Items);
        using var scope = app.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<GrandmastersDbContext>();
        Assert.Empty(await database.CartItems.ToListAsync());
        Assert.Equal(5, (await database.Inventory.SingleAsync(i => i.ProductVariantId == app.InStock.ProductVariantId)).Quantity);
    }

    [Fact]
    public async Task Cart_CannotReadUpdateOrRemoveAnotherUsersItems()
    {
        using var app = new ShoppingFactory();
        await app.InitializeAsync();
        using var alice = app.Client(app.Alice);
        using var bob = app.Client(app.Bob);
        var cart = await ReadCart(await app.Add(alice, 1));
        var id = Assert.Single(cart.Items).CartItemId;
        Assert.Empty((await bob.GetFromJsonAsync<CartDto>("/api/v1/cart"))!.Items);
        Assert.Equal(HttpStatusCode.NotFound,
            (await bob.PutAsJsonAsync($"/api/v1/cart/items/{id}", new { quantity = 2 })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await bob.DeleteAsync($"/api/v1/cart/items/{id}")).StatusCode);
        Assert.Single((await alice.GetFromJsonAsync<CartDto>("/api/v1/cart"))!.Items);
    }

    [Theory]
    [InlineData(0)] [InlineData(-1)] [InlineData(100)]
    public async Task Cart_RejectsInvalidQuantities(int quantity)
    {
        using var app = new ShoppingFactory();
        await app.InitializeAsync();
        using var client = app.Client(app.Alice);
        Assert.Equal(HttpStatusCode.BadRequest, (await app.Add(client, quantity)).StatusCode);
        Assert.Empty((await client.GetFromJsonAsync<CartDto>("/api/v1/cart"))!.Items);
        var cart = await ReadCart(await app.Add(client, 1));
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PutAsJsonAsync(
            $"/api/v1/cart/items/{cart.Items[0].CartItemId}", new { quantity })).StatusCode);
        Assert.Equal(1, (await client.GetFromJsonAsync<CartDto>("/api/v1/cart"))!.TotalQuantity);
    }

    [Fact]
    public async Task Cart_RejectsOverStockAndSoldOutOptionsWithoutChangingCart()
    {
        using var app = new ShoppingFactory();
        await app.InitializeAsync();
        using var client = app.Client(app.Alice);
        Assert.Equal(HttpStatusCode.Conflict, (await app.Add(client, 6)).StatusCode);
        Assert.Empty((await client.GetFromJsonAsync<CartDto>("/api/v1/cart"))!.Items);
        await ReadCart(await app.Add(client, 4));
        Assert.Equal(HttpStatusCode.Conflict, (await app.Add(client, 2)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/v1/cart/items", new
        {
            productId = app.Product.ProductId, productVariantId = app.SoldOut.ProductVariantId, quantity = 1
        })).StatusCode);
        Assert.Equal(4, (await client.GetFromJsonAsync<CartDto>("/api/v1/cart"))!.TotalQuantity);
    }

    [Fact]
    public async Task Cart_RejectsMissingProductsAndMismatchedOptions()
    {
        using var app = new ShoppingFactory();
        await app.InitializeAsync();
        using var client = app.Client(app.Alice);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsJsonAsync("/api/v1/cart/items", new
        { productId = 999999, productVariantId = app.InStock.ProductVariantId, quantity = 1 })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsJsonAsync("/api/v1/cart/items", new
        { productId = app.Product.ProductId, productVariantId = 999999, quantity = 1 })).StatusCode);
        Assert.Empty((await client.GetFromJsonAsync<CartDto>("/api/v1/cart"))!.Items);
    }

    [Fact]
    public async Task Cart_RefreshReflectsChangedPriceAndStock()
    {
        using var app = new ShoppingFactory();
        await app.InitializeAsync();
        using var client = app.Client(app.Alice);
        await ReadCart(await app.Add(client, 3));
        using (var scope = app.Services.CreateScope())
        {
            var database = scope.ServiceProvider.GetRequiredService<GrandmastersDbContext>();
            (await database.ProductVariants.SingleAsync(v => v.ProductVariantId == app.InStock.ProductVariantId)).Price = 200m;
            (await database.Inventory.SingleAsync(i => i.ProductVariantId == app.InStock.ProductVariantId)).Quantity = 1;
            await database.SaveChangesAsync();
        }
        var cart = (await client.GetFromJsonAsync<CartDto>("/api/v1/cart"))!;
        Assert.Equal(600m, cart.Subtotal);
        Assert.False(cart.Items[0].IsAvailable);
        Assert.Equal(1, cart.Items[0].StockQuantity);
    }

    [Fact]
    public async Task Cart_RejectsTokenForDeletedAccount()
    {
        using var app = new ShoppingFactory();
        await app.InitializeAsync();
        using var client = app.Client(app.Alice);
        using (var scope = app.Services.CreateScope())
        {
            var database = scope.ServiceProvider.GetRequiredService<GrandmastersDbContext>();
            database.Users.Remove((await database.Users.FindAsync(app.Alice.UserId))!);
            await database.SaveChangesAsync();
        }
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/cart")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await app.Add(client, 1)).StatusCode);
    }

    private static async Task<CartDto> ReadCart(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CartDto>())!;
    }

    private sealed class ShoppingFactory : WebApplicationFactory<Program>
    {
        private readonly string _database = Path.Combine(Path.GetTempPath(), $"sen371-shopping-{Guid.NewGuid():N}.db");
        public User Alice { get; } = new() { Email = "alice@example.test", PasswordHash = "unused", Role = "Customer" };
        public User Bob { get; } = new() { Email = "bob@example.test", PasswordHash = "unused", Role = "Customer" };
        public ProductVariant InStock { get; } = new() { Name = "Walnut", Price = 125.50m, Inventory = new Inventory { Quantity = 5 } };
        public ProductVariant SoldOut { get; } = new() { Name = "Ebony", Price = 150m, Inventory = new Inventory { Quantity = 0 } };
        public Product Product { get; } = new()
        {
            Name = "Tournament board", Slug = "test-tournament-board", Description = "A test board.",
            Price = 100m, Category = new Category { Name = "Boards" },
            Images = new List<ProductImage> { new() { ImageUrl = "/images/Shopping-plain-chess-board.jpg" } }
        };

        protected override IHost CreateHost(IHostBuilder builder)
        {
            // Program reads JWT settings before Build. Host configuration is supplied
            // to the minimal-host entry point early enough, without process-wide secrets.
            builder.ConfigureHostConfiguration(config => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SigningKey"] = "shopping-tests-only-do-not-use-outside-tests-2026",
                ["Database:ApplyMigrationsOnStartup"] = "false"
            }));
            return base.CreateHost(builder);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<GrandmastersDbContext>();
                services.RemoveAll<DbContextOptions<GrandmastersDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<GrandmastersDbContext>>();
                services.AddDbContext<GrandmastersDbContext>(options => options.UseSqlite($"Data Source={_database};Pooling=False"));
            });
        }

        public async Task InitializeAsync()
        {
            using var scope = Services.CreateScope();
            var database = scope.ServiceProvider.GetRequiredService<GrandmastersDbContext>();
            await database.Database.EnsureCreatedAsync();
            database.Users.AddRange(Alice, Bob);
            Product.Variants.Add(InStock); Product.Variants.Add(SoldOut);
            database.Products.Add(Product);
            await database.SaveChangesAsync();
        }

        public HttpClient Client(User? user = null)
        {
            var client = CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
            if (user is not null)
            {
                using var scope = Services.CreateScope();
                var token = scope.ServiceProvider.GetRequiredService<ITokenService>().CreateToken(user);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            }
            return client;
        }

        public Task<HttpResponseMessage> Add(HttpClient client, int quantity) => client.PostAsJsonAsync("/api/v1/cart/items", new
        { productId = Product.ProductId, productVariantId = InStock.ProductVariantId, quantity });

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing && File.Exists(_database)) File.Delete(_database);
        }
    }
}
