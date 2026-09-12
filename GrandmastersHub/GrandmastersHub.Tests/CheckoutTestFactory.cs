using System.Net.Http.Headers;
using System.Net.Http.Json;
using GrandmastersHub.Application.DTOs.Orders;
using GrandmastersHub.Application.Interfaces;
using GrandmastersHub.Domain.Entities;
using GrandmastersHub.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace GrandmastersHub.Tests;

internal sealed class CheckoutTestFactory : WebApplicationFactory<Program>
{
    private readonly string _name = $"sen371_checkout_{Guid.NewGuid():N}";
    private string DatabaseFile => Path.Combine(Path.GetTempPath(), $"{_name}.db");
    private readonly string? _sqlServer = Environment.GetEnvironmentVariable("CHECKOUT_SQLSERVER_CONNECTION");
    public bool UsesSqlServer => !string.IsNullOrWhiteSpace(_sqlServer);
    public FailOrderSaveInterceptor SaveFailure { get; } = new();
    public User Alice { get; } = new() { Email = "alice@example.test", PasswordHash = "unused", Role = "Customer" };
    public User Bob { get; } = new() { Email = "bob@example.test", PasswordHash = "unused", Role = "Customer" };
    public ProductVariant Variant { get; } = new() { Name = "Walnut", Price = 125.50m, Inventory = new Inventory { Quantity = 5 } };
    public Product Product { get; } = new()
    {
        Name = "Test tournament board", Slug = "checkout-test-board", Price = 100m,
        Category = new Category { Name = "Checkout test boards" },
        Images = new List<ProductImage> { new() { ImageUrl = "/images/Shopping-plain-chess-board.jpg" } }
    };

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(config => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:SigningKey"] = "checkout-tests-only-do-not-use-outside-tests-2026",
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
            services.AddDbContext<GrandmastersDbContext>(options =>
            {
                if (UsesSqlServer)
                {
                    // Always use our own disposable database, never the supplied initial catalog.
                    var connection = new SqlConnectionStringBuilder(_sqlServer!)
                    { InitialCatalog = _name, Pooling = false };
                    options.UseSqlServer(connection.ConnectionString);
                }
                else options.UseSqlite($"Data Source={DatabaseFile};Pooling=False");
                options.AddInterceptors(SaveFailure);
            });
        });
    }

    public async Task InitializeAsync()
    {
        await Edit(async database =>
        {
            if (UsesSqlServer) await database.Database.MigrateAsync();
            else await database.Database.EnsureCreatedAsync();
            database.Users.AddRange(Alice, Bob);
            Product.Variants.Add(Variant);
            database.Products.Add(Product);
            await database.SaveChangesAsync();
        });
    }

    public async Task Edit(Func<GrandmastersDbContext, Task> operation)
    {
        using var scope = Services.CreateScope();
        await operation(scope.ServiceProvider.GetRequiredService<GrandmastersDbContext>());
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

    public async Task Add(HttpClient client, int quantity = 2)
    {
        var response = await client.PostAsJsonAsync("/api/v1/cart/items", new
        { productId = Product.ProductId, productVariantId = Variant.ProductVariantId, quantity });
        response.EnsureSuccessStatusCode();
    }

    public static async Task<PlaceOrderRequest> Request(HttpClient client, Guid? key = null)
    {
        var preview = await client.GetFromJsonAsync<CheckoutPreview>("/api/v1/orders/checkout");
        return new PlaceOrderRequest
        {
            CheckoutKey = key ?? Guid.NewGuid(), CartFingerprint = preview!.CartFingerprint,
            ShippingAddress = new DeliveryAddress
            {
                RecipientName = "Alice Test", StreetAddress = "12 Test Street", City = "Pretoria",
                Province = "Gauteng", PostalCode = "0002", Country = "South Africa"
            }
        };
    }

    public async Task AssertState(int orders, int quantity, int cartQuantity)
    {
        await Edit(async database =>
        {
            Assert.Equal(orders, await database.Orders.CountAsync());
            Assert.Equal(orders, await database.Payments.CountAsync());
            Assert.Equal(quantity, (await database.Inventory.SingleAsync(i => i.ProductVariantId == Variant.ProductVariantId)).Quantity);
            Assert.Equal(cartQuantity, await database.CartItems.SumAsync(i => i.Quantity));
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && UsesSqlServer)
        {
            using var scope = Services.CreateScope();
            scope.ServiceProvider.GetRequiredService<GrandmastersDbContext>().Database.EnsureDeleted();
        }
        base.Dispose(disposing);
        if (disposing && !UsesSqlServer && File.Exists(DatabaseFile)) File.Delete(DatabaseFile);
    }

    internal sealed class FailOrderSaveInterceptor : SaveChangesInterceptor
    {
        public bool FailNextOrder { get; set; }
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
            InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (FailNextOrder && eventData.Context!.ChangeTracker.Entries<Order>().Any(e => e.State == EntityState.Added))
            {
                FailNextOrder = false;
                throw new InvalidOperationException("Injected order save failure for transaction rollback test.");
            }
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
