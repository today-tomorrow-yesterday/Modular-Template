using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Sales.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceIntRefWithGuidPublicId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_on_lot_homes_cache_ref_on_lot_home_id",
                schema: "cache",
                table: "on_lot_homes_cache");

            migrationBuilder.DropIndex(
                name: "ix_land_parcels_cache_ref_land_parcel_id",
                schema: "cache",
                table: "land_parcels_cache");

            migrationBuilder.DropColumn(
                name: "ref_on_lot_home_id",
                schema: "cache",
                table: "on_lot_homes_cache");

            migrationBuilder.DropColumn(
                name: "ref_land_parcel_id",
                schema: "cache",
                table: "land_parcels_cache");

            migrationBuilder.AddColumn<Guid>(
                name: "ref_public_on_lot_home_id",
                schema: "cache",
                table: "on_lot_homes_cache",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ref_public_land_parcel_id",
                schema: "cache",
                table: "land_parcels_cache",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_on_lot_homes_cache_ref_public_on_lot_home_id",
                schema: "cache",
                table: "on_lot_homes_cache",
                column: "ref_public_on_lot_home_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_land_parcels_cache_ref_public_land_parcel_id",
                schema: "cache",
                table: "land_parcels_cache",
                column: "ref_public_land_parcel_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_on_lot_homes_cache_ref_public_on_lot_home_id",
                schema: "cache",
                table: "on_lot_homes_cache");

            migrationBuilder.DropIndex(
                name: "ix_land_parcels_cache_ref_public_land_parcel_id",
                schema: "cache",
                table: "land_parcels_cache");

            migrationBuilder.DropColumn(
                name: "ref_public_on_lot_home_id",
                schema: "cache",
                table: "on_lot_homes_cache");

            migrationBuilder.DropColumn(
                name: "ref_public_land_parcel_id",
                schema: "cache",
                table: "land_parcels_cache");

            migrationBuilder.AddColumn<int>(
                name: "ref_on_lot_home_id",
                schema: "cache",
                table: "on_lot_homes_cache",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ref_land_parcel_id",
                schema: "cache",
                table: "land_parcels_cache",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_on_lot_homes_cache_ref_on_lot_home_id",
                schema: "cache",
                table: "on_lot_homes_cache",
                column: "ref_on_lot_home_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_land_parcels_cache_ref_land_parcel_id",
                schema: "cache",
                table: "land_parcels_cache",
                column: "ref_land_parcel_id",
                unique: true);
        }
    }
}
