using GrandmastersHub.Domain.Entities;
using GrandmastersHub.Infrastructure.Data;
using GrandmastersHub.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GrandmastersHub.Tests;

public class CategoryRepositoryTests
{
    private GrandmastersDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<GrandmastersDbContext>()
            .UseSqlServer(
                "Server=localhost;Database=GrandmastersHubDb;Trusted_Connection=True;TrustServerCertificate=True;")
            .Options;

        return new GrandmastersDbContext(options);
    }

    [Fact]
    public async Task Category_CRUD_ShouldWork()
    {
        await using var context = TestDbContextFactory.Create();

        var repository = new CategoryRepository(context);

        // CREATE
        var category = new Category
        {
            Name = $"Test Category {Guid.NewGuid()}"
        };

        await repository.AddAsync(category);

        Assert.True(category.CategoryId > 0);

        // READ
        var createdCategory =
            await repository.GetByIdAsync(category.CategoryId);

        Assert.NotNull(createdCategory);
        Assert.Equal(category.CategoryId, createdCategory!.CategoryId);

        // UPDATE
        createdCategory.Name = $"Updated Category {Guid.NewGuid()}";

        await repository.UpdateAsync(createdCategory);

        var updatedCategory =
            await repository.GetByIdAsync(category.CategoryId);

        Assert.NotNull(updatedCategory);
        Assert.Equal(createdCategory.Name, updatedCategory!.Name);

        // DELETE
        await repository.DeleteAsync(category.CategoryId);

        var deletedCategory =
            await repository.GetByIdAsync(category.CategoryId);

        Assert.Null(deletedCategory);
    }
}