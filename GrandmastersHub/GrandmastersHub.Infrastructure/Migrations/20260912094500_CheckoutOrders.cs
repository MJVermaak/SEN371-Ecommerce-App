using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrandmastersHub.Infrastructure.Migrations;

public partial class CheckoutOrders : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "CheckoutKey", table: "Orders",
            type: "nvarchar(36)", maxLength: 36, nullable: true);
        migrationBuilder.AddColumn<string>(name: "ReceiptJson", table: "Orders",
            type: "nvarchar(max)", nullable: true);
        migrationBuilder.CreateIndex(name: "IX_Orders_UserId_CheckoutKey", table: "Orders",
            columns: new[] { "UserId", "CheckoutKey" }, unique: true, filter: "[CheckoutKey] IS NOT NULL");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Do not silently discard receipt addresses or duplicate-submission protection.
        migrationBuilder.Sql("""
            IF EXISTS (SELECT 1 FROM [Orders] WHERE [CheckoutKey] IS NOT NULL OR [ReceiptJson] IS NOT NULL)
                THROW 51010, 'Checkout orders exist. Preserve their receipts before rolling back this migration.', 1;
            """);
        migrationBuilder.DropIndex(name: "IX_Orders_UserId_CheckoutKey", table: "Orders");
        migrationBuilder.DropColumn(name: "CheckoutKey", table: "Orders");
        migrationBuilder.DropColumn(name: "ReceiptJson", table: "Orders");
    }
}
