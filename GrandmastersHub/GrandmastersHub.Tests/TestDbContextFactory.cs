using GrandmastersHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GrandmastersHub.Tests;

public static class TestDbContextFactory
{
    private const string ConnectionString =
        "Server=localhost;Database=GrandmastersHubDb;Trusted_Connection=True;TrustServerCertificate=True;";

    public static GrandmastersDbContext Create()
    {
        var options = new DbContextOptionsBuilder<GrandmastersDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        return new GrandmastersDbContext(options);
    }
}