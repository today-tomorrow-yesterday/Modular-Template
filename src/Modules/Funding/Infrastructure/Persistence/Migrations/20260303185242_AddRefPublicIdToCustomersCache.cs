using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Modules.Funding.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRefPublicIdToCustomersCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_funding_requests_ref_customer_id",
                schema: "fundings",
                table: "funding_requests");

            migrationBuilder.DropColumn(
                name: "ref_customer_id",
                schema: "fundings",
                table: "pending_funding_requests");

            migrationBuilder.DropColumn(
                name: "ref_customer_id",
                schema: "fundings",
                table: "funding_requests");

            migrationBuilder.AddColumn<Guid>(
                name: "ref_customer_public_id",
                schema: "fundings",
                table: "pending_funding_requests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ref_customer_public_id",
                schema: "fundings",
                table: "funding_requests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "id",
                schema: "cache",
                table: "customers_cache",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);

            migrationBuilder.AddColumn<Guid>(
                name: "ref_public_id",
                schema: "cache",
                table: "customers_cache",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_funding_requests_ref_customer_public_id",
                schema: "fundings",
                table: "funding_requests",
                column: "ref_customer_public_id",
                filter: "ref_customer_public_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_customers_cache_ref_public_id",
                schema: "cache",
                table: "customers_cache",
                column: "ref_public_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_funding_requests_ref_customer_public_id",
                schema: "fundings",
                table: "funding_requests");

            migrationBuilder.DropIndex(
                name: "ix_customers_cache_ref_public_id",
                schema: "cache",
                table: "customers_cache");

            migrationBuilder.DropColumn(
                name: "ref_customer_public_id",
                schema: "fundings",
                table: "pending_funding_requests");

            migrationBuilder.DropColumn(
                name: "ref_customer_public_id",
                schema: "fundings",
                table: "funding_requests");

            migrationBuilder.DropColumn(
                name: "ref_public_id",
                schema: "cache",
                table: "customers_cache");

            migrationBuilder.AddColumn<int>(
                name: "ref_customer_id",
                schema: "fundings",
                table: "pending_funding_requests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ref_customer_id",
                schema: "fundings",
                table: "funding_requests",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "id",
                schema: "cache",
                table: "customers_cache",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);

            migrationBuilder.CreateIndex(
                name: "ix_funding_requests_ref_customer_id",
                schema: "fundings",
                table: "funding_requests",
                column: "ref_customer_id",
                filter: "ref_customer_id IS NOT NULL");
        }
    }
}
