using GrandmastersHub.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Xunit;

namespace GrandmastersHub.Tests;

public class GlobalErrorHandlingMiddlewareTests
{
    [Fact]
    public async Task Middleware_ShouldReturn500_WhenExceptionOccurs()
    {
        // Arrange
        var context = new DefaultHttpContext();

        context.Response.Body = new MemoryStream();

        var loggerFactory = LoggerFactory.Create(builder => { });

        var logger =
            loggerFactory.CreateLogger<GlobalErrorHandlingMiddleware>();

        RequestDelegate next = _ =>
        {
            throw new Exception("Test exception");
        };

        var middleware =
            new GlobalErrorHandlingMiddleware(next, logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(
            StatusCodes.Status500InternalServerError,
            context.Response.StatusCode);

        Assert.Equal(
            "application/json",
            context.Response.ContentType);

        context.Response.Body.Seek(0, SeekOrigin.Begin);

        using var reader =
            new StreamReader(context.Response.Body);

        var responseBody =
            await reader.ReadToEndAsync();

        Assert.Contains(
            "An unexpected error occurred.",
            responseBody);

        // Internal exception details should NOT be exposed
        // to the client.
        Assert.DoesNotContain(
            "Test exception",
            responseBody);
    }
}