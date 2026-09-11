using System.Net;
using GrandmastersHub.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GrandmastersHub.Tests;

public sealed class DatabaseStartupTests
{
    private const string DatabaseResolved = "Database startup regression test resolved the database.";

    [Theory]
    [InlineData("Development", "false")]
    [InlineData("Testing", "true")]
    [InlineData("Production", "true")]
    public async Task Startup_DoesNotResolveDatabase_WhenDisabledOrOutsideDevelopment(
        string environment, string setting)
    {
        using var factory = CreateFactory(environment, setting);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        using var response = await client.GetAsync("/__database_startup_test__");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("true")]
    public void Startup_ResolvesDatabase_InDevelopmentByDefaultOrWhenEnabled(string? setting)
    {
        using var factory = CreateFactory("Development", setting);

        // The registered factory throws before a connection is opened. This also
        // verifies that a database initialization failure cannot be swallowed.
        var exception = Assert.ThrowsAny<Exception>(() =>
        {
            using var client = factory.CreateClient();
        });

        Assert.Contains(DatabaseResolved, exception.ToString());
    }

    private static WebApplicationFactory<Program> CreateFactory(string environment, string? setting)
    {
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment(environment);
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Database:ApplyMigrationsOnStartup"] = setting,
                    ["Jwt:Issuer"] = "GrandmastersHub.Api",
                    ["Jwt:Audience"] = "GrandmastersHub.Client",
                    ["Jwt:SigningKey"] = "startup-tests-only-not-a-production-signing-key",
                    ["Jwt:ExpiryMinutes"] = "5"
                });
            });
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<GrandmastersDbContext>();
                services.AddScoped<GrandmastersDbContext>(_ =>
                    throw new InvalidOperationException(DatabaseResolved));
            });
        });
    }
}
