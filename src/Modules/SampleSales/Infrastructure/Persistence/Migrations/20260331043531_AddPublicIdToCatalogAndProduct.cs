using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Modules.SampleSales.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicIdToCatalogAndProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_orders_cache_customer_id",
                schema: "cache",
                table: "orders_cache");

            migrationBuilder.DropColumn(
                name: "customer_id",
                schema: "cache",
                table: "orders_cache");

            migrationBuilder.AddColumn<Guid>(
                name: "public_id",
                schema: "sample",
                table: "products",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<int>(
                name: "id",
                schema: "cache",
                table: "orders_cache",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);

            migrationBuilder.AddColumn<Guid>(
                name: "ref_public_customer_id",
                schema: "cache",
                table: "orders_cache",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ref_public_id",
                schema: "cache",
                table: "orders_cache",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "public_id",
                schema: "sample",
                table: "catalogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_products_public_id",
                schema: "sample",
                table: "products",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_orders_cache_ref_public_customer_id",
                schema: "cache",
                table: "orders_cache",
                column: "ref_public_customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_cache_ref_public_id",
                schema: "cache",
                table: "orders_cache",
                column: "ref_public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_catalogs_public_id",
                schema: "sample",
                table: "catalogs",
                column: "public_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_products_public_id",
                schema: "sample",
                table: "products");

            migrationBuilder.DropIndex(
                name: "ix_orders_cache_ref_public_customer_id",
                schema: "cache",
                table: "orders_cache");

            migrationBuilder.DropIndex(
                name: "ix_orders_cache_ref_public_id",
                schema: "cache",
                table: "orders_cache");

            migrationBuilder.DropIndex(
                name: "ix_catalogs_public_id",
                schema: "sample",
                table: "catalogs");

            migrationBuilder.DropColumn(
                name: "public_id",
                schema: "sample",
                table: "products");

            migrationBuilder.DropColumn(
                name: "ref_public_customer_id",
                schema: "cache",
                table: "orders_cache");

            migrationBuilder.DropColumn(
                name: "ref_public_id",
                schema: "cache",
                table: "orders_cache");

            migrationBuilder.DropColumn(
                name: "public_id",
                schema: "sample",
                table: "catalogs");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                schema: "cache",
                table: "orders_cache",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);

            migrationBuilder.AddColumn<int>(
                name: "customer_id",
                schema: "cache",
                table: "orders_cache",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_orders_cache_customer_id",
                schema: "cache",
                table: "orders_cache",
                column: "customer_id");
        }
    }
}
