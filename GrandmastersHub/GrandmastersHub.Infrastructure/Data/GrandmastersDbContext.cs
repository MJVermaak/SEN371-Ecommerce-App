using GrandmastersHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GrandmastersHub.Infrastructure.Data;

public sealed class GrandmastersDbContext(DbContextOptions<GrandmastersDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GrandmastersDbContext).Assembly);
}
