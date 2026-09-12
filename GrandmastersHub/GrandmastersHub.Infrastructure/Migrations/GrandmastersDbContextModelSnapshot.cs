using GrandmastersHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace GrandmastersHub.Infrastructure.Migrations;

[DbContext(typeof(GrandmastersDbContext))]
partial class GrandmastersDbContextModelSnapshot : ModelSnapshot
{
    // Reuse frozen metadata, not the live DbContext model. Future EF scaffolding replaces this snapshot.
    protected override void BuildModel(ModelBuilder modelBuilder) => CheckoutOrders.BuildCheckoutModel(modelBuilder);
}
