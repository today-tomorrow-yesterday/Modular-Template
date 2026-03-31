using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Inventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DropSaleSummaryCacheAndReclassifyEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sale_summaries_cache",
                schema: "cache");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sale_summaries_cache",
                schema: "cache",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    current_retail_price = table.Column<string>(type: "text", nullable: true),
                    customer_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    last_synced_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    original_retail_price = table.Column<string>(type: "text", nullable: true),
                    received_in_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ref_stock_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    sale_public_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sale_summaries_cache", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_sale_summaries_cache_ref_stock_number",
                schema: "cache",
                table: "sale_summaries_cache",
                column: "ref_stock_number",
                unique: true);
        }
    }
}
