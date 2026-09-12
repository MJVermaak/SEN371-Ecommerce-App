using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GrandmastersHub.Tests;

// Run in both CI database jobs. These tests also verify that cleanup actually
// removes the test database rather than merely hiding a disposal exception.
public sealed class CheckoutFactoryDisposalTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Dispose_DeletesDatabaseAndCanBeRepeated(bool asyncFirst)
    {
        using var app = new CheckoutTestFactory();
        await app.InitializeAsync();
        var connectionString = await ConnectionString(app);
        await AssertDatabaseExists(app.UsesSqlServer, connectionString, true);

        if (asyncFirst) await app.DisposeAsync();
        else app.Dispose();

        await AssertDatabaseExists(app.UsesSqlServer, connectionString, false);
        // Exercise both orders of sync/async disposal, including the final using.
        app.Dispose();
        await app.DisposeAsync();
        await AssertDatabaseExists(app.UsesSqlServer, connectionString, false);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Dispose_UninitializedFactoryCanBeRepeated(bool asyncFirst)
    {
        using var app = new CheckoutTestFactory();
        if (asyncFirst) await app.DisposeAsync();
        else app.Dispose();
        app.Dispose();
        await app.DisposeAsync();
    }

    [Fact]
    public async Task Dispose_DeletesDatabaseAfterSeedFailure()
    {
        using var app = new CheckoutTestFactory();
        // Both providers enforce this required column after the schema is created.
        app.Product.Name = null!;
        await Assert.ThrowsAsync<DbUpdateException>(() => app.InitializeAsync());
        var connectionString = await ConnectionString(app);
        await AssertDatabaseExists(app.UsesSqlServer, connectionString, true);
        await app.DisposeAsync();
        await AssertDatabaseExists(app.UsesSqlServer, connectionString, false);
    }

    [Fact]
    public async Task Dispose_DoesNotDeleteAnotherFactoryDatabase()
    {
        using var first = new CheckoutTestFactory();
        using var second = new CheckoutTestFactory();
        await first.InitializeAsync();
        await second.InitializeAsync();
        var firstConnection = await ConnectionString(first);
        var secondConnection = await ConnectionString(second);
        Assert.NotEqual(firstConnection, secondConnection);

        await first.DisposeAsync();

        await AssertDatabaseExists(first.UsesSqlServer, firstConnection, false);
        await AssertDatabaseExists(second.UsesSqlServer, secondConnection, true);
        await second.AssertState(0, 5, 0);
    }

    private static async Task<string> ConnectionString(CheckoutTestFactory app)
    {
        string? connectionString = null;
        await app.Edit(database =>
        {
            connectionString = database.Database.GetConnectionString();
            return Task.CompletedTask;
        });
        Assert.NotNull(connectionString);
        return connectionString;
    }

    private static async Task AssertDatabaseExists(bool sqlServer, string connectionString, bool expected)
    {
        if (!sqlServer)
        {
            var sqlite = new SqliteConnectionStringBuilder(connectionString);
            Assert.Equal(expected, File.Exists(sqlite.DataSource));
            return;
        }

        // Query master instead of connecting to (or recreating) the deleted database.
        var settings = new SqlConnectionStringBuilder(connectionString);
        var databaseName = settings.InitialCatalog;
        Assert.StartsWith("sen371_checkout_", databaseName);
        settings.InitialCatalog = "master";
        await using var connection = new SqlConnection(settings.ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name = @name";
        command.Parameters.Add("@name", SqlDbType.NVarChar, 128).Value = databaseName;
        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        Assert.Equal(expected ? 1 : 0, count);
    }
}
