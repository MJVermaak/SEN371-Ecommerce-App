using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GrandmastersHub.Tests;

public class DatabaseConnectionTests
{
    [Fact]
    public async Task Database_CanConnect()
    {
        await using var context = TestDbContextFactory.Create();

        var canConnect = await context.Database.CanConnectAsync();

        Assert.True(canConnect);
    }
}