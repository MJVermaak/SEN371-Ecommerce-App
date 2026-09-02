using GrandmastersHub.Domain.Entities;
using GrandmastersHub.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GrandmastersHub.Tests;

public sealed class CommercePersistenceTests
{
    [Fact]
    public async Task IntegratedCommerceModel_CreatesACompleteSqliteSchema()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<GrandmastersDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new GrandmastersDbContext(options);
        Assert.True(await context.Database.EnsureCreatedAsync());

        var tableNames = new List<string>();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table'";
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tableNames.Add(reader.GetString(0));
        }

        Assert.Contains("Users", tableNames);
        Assert.Contains("Categories", tableNames);
        Assert.Contains("Products", tableNames);
        Assert.Contains("Carts", tableNames);
        Assert.Contains("CartItems", tableNames);
        Assert.Contains("Orders", tableNames);
        Assert.Contains("OrderItems", tableNames);
        Assert.Contains("Reviews", tableNames);
    }

    [Fact]
    public void IntegratedCommerceModel_UsesGuidForeignKeysForAuthenticatedUsers()
    {
        Assert.Equal(typeof(Guid), typeof(Cart).GetProperty(nameof(Cart.UserId))!.PropertyType);
        Assert.Equal(typeof(Guid), typeof(Order).GetProperty(nameof(Order.UserId))!.PropertyType);
        Assert.Equal(typeof(Guid), typeof(Review).GetProperty(nameof(Review.UserId))!.PropertyType);
    }
}
