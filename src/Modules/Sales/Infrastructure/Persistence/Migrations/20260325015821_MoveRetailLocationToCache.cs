using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Sales.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MoveRetailLocationToCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the old FK constraint
            migrationBuilder.DropForeignKey(
                name: "fk_sales_retail_locations_retail_location_id",
                schema: "sales",
                table: "sales");

            // Move table: rename + change schema (preserves all data)
            migrationBuilder.RenameTable(
                name: "retail_locations",
                schema: "sales",
                newName: "retail_location_cache",
                newSchema: "cache");

            // Drop audit columns that cache entities don't have
            migrationBuilder.DropColumn(name: "created_at_utc", schema: "cache", table: "retail_location_cache");
            migrationBuilder.DropColumn(name: "created_by_user_id", schema: "cache", table: "retail_location_cache");
            migrationBuilder.DropColumn(name: "modified_at_utc", schema: "cache", table: "retail_location_cache");
            migrationBuilder.DropColumn(name: "modified_by_user_id", schema: "cache", table: "retail_location_cache");

            // Add cache-specific column
            migrationBuilder.AddColumn<DateTime>(
                name: "last_synced_at_utc",
                schema: "cache",
                table: "retail_location_cache",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            // Rename PK and index constraints
            migrationBuilder.Sql(
                "ALTER INDEX cache.pk_retail_locations RENAME TO pk_retail_location_cache");
            migrationBuilder.Sql(
                "ALTER INDEX cache.ix_retail_locations_ref_home_center_number RENAME TO ix_retail_location_cache_ref_home_center_number");

            // Re-add FK pointing to the new location
            migrationBuilder.AddForeignKey(
                name: "fk_sales_retail_location_cache_retail_location_id",
                schema: "sales",
                table: "sales",
                column: "retail_location_id",
                principalSchema: "cache",
                principalTable: "retail_location_cache",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_sales_retail_location_cache_retail_location_id",
                schema: "sales",
                table: "sales");

            migrationBuilder.DropTable(
                name: "retail_location_cache",
                schema: "cache");

            migrationBuilder.CreateTable(
                name: "retail_locations",
                schema: "sales",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    location_type = table.Column<string>(type: "text", nullable: false),
                    modified_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "text", nullable: false),
                    organization_metadata = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    ref_home_center_number = table.Column<int>(type: "integer", nullable: true),
                    state_code = table.Column<string>(type: "text", nullable: false),
                    zip = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_retail_locations", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_retail_locations_ref_home_center_number",
                schema: "sales",
                table: "retail_locations",
                column: "ref_home_center_number",
                unique: true,
                filter: "ref_home_center_number IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "fk_sales_retail_locations_retail_location_id",
                schema: "sales",
                table: "sales",
                column: "retail_location_id",
                principalSchema: "sales",
                principalTable: "retail_locations",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
