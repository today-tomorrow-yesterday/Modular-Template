using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Sales.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductRemovedAndCacheSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_product_removed_from_inventory",
                schema: "packages",
                table: "package_lines",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_removed_from_inventory",
                schema: "cache",
                table: "on_lot_homes_cache",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_removed_from_inventory",
                schema: "cache",
                table: "land_parcels_cache",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_product_removed_from_inventory",
                schema: "packages",
                table: "package_lines");

            migrationBuilder.DropColumn(
                name: "is_removed_from_inventory",
                schema: "cache",
                table: "on_lot_homes_cache");

            migrationBuilder.DropColumn(
                name: "is_removed_from_inventory",
                schema: "cache",
                table: "land_parcels_cache");
        }
    }
}
