using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Inventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicIdToInventoryEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "public_id",
                schema: "inventories",
                table: "on_lot_homes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "public_id",
                schema: "inventories",
                table: "land_parcels",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_on_lot_homes_public_id",
                schema: "inventories",
                table: "on_lot_homes",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_land_parcels_public_id",
                schema: "inventories",
                table: "land_parcels",
                column: "public_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_on_lot_homes_public_id",
                schema: "inventories",
                table: "on_lot_homes");

            migrationBuilder.DropIndex(
                name: "ix_land_parcels_public_id",
                schema: "inventories",
                table: "land_parcels");

            migrationBuilder.DropColumn(
                name: "public_id",
                schema: "inventories",
                table: "on_lot_homes");

            migrationBuilder.DropColumn(
                name: "public_id",
                schema: "inventories",
                table: "land_parcels");
        }
    }
}
