using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Modules.SampleOrders.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceCustomerAndOrderAggregates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_products_cache_is_active",
                schema: "cache",
                table: "products_cache");

            migrationBuilder.DropIndex(
                name: "ix_order_lines_product_id",
                schema: "orders",
                table: "order_lines");

            migrationBuilder.DropIndex(
                name: "ix_customers_email",
                schema: "orders",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "email",
                schema: "orders",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "name",
                schema: "orders",
                table: "customers");

            migrationBuilder.RenameColumn(
                name: "product_id",
                schema: "orders",
                table: "order_lines",
                newName: "sort_order");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                schema: "cache",
                table: "products_cache",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);

            migrationBuilder.AddColumn<Guid>(
                name: "ref_public_id",
                schema: "cache",
                table: "products_cache",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "public_id",
                schema: "orders",
                table: "orders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "details",
                schema: "orders",
                table: "order_lines",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "line_type",
                schema: "orders",
                table: "order_lines",
                type: "character varying(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "product_cache_id",
                schema: "orders",
                table: "order_lines",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "date_of_birth",
                schema: "orders",
                table: "customers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "first_name",
                schema: "orders",
                table: "customers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "last_name",
                schema: "orders",
                table: "customers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "middle_name",
                schema: "orders",
                table: "customers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "public_id",
                schema: "orders",
                table: "customers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "status",
                schema: "orders",
                table: "customers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "customer_addresses",
                schema: "orders",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    customer_id = table.Column<int>(type: "integer", nullable: false),
                    address_line1 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    address_line2 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    state = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    country = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customer_addresses", x => x.id);
                    table.ForeignKey(
                        name: "fk_customer_addresses_customers_customer_id",
                        column: x => x.customer_id,
                        principalSchema: "orders",
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customer_contacts",
                schema: "orders",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    customer_id = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    value = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customer_contacts", x => x.id);
                    table.ForeignKey(
                        name: "fk_customer_contacts_customers_customer_id",
                        column: x => x.customer_id,
                        principalSchema: "orders",
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shipping_addresses",
                schema: "orders",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    order_id = table.Column<int>(type: "integer", nullable: false),
                    address_line1 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    address_line2 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    state = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    country = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shipping_addresses", x => x.id);
                    table.ForeignKey(
                        name: "fk_shipping_addresses_orders_order_id",
                        column: x => x.order_id,
                        principalSchema: "orders",
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_products_cache_ref_public_id",
                schema: "cache",
                table: "products_cache",
                column: "ref_public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_orders_public_id",
                schema: "orders",
                table: "orders",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_order_lines_product_cache_id",
                schema: "orders",
                table: "order_lines",
                column: "product_cache_id");

            migrationBuilder.CreateIndex(
                name: "ix_customers_public_id",
                schema: "orders",
                table: "customers",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_customer_addresses_customer_id",
                schema: "orders",
                table: "customer_addresses",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_customer_contacts_customer_type_value",
                schema: "orders",
                table: "customer_contacts",
                columns: new[] { "customer_id", "type", "value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_shipping_addresses_order_id",
                schema: "orders",
                table: "shipping_addresses",
                column: "order_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customer_addresses",
                schema: "orders");

            migrationBuilder.DropTable(
                name: "customer_contacts",
                schema: "orders");

            migrationBuilder.DropTable(
                name: "shipping_addresses",
                schema: "orders");

            migrationBuilder.DropIndex(
                name: "ix_products_cache_ref_public_id",
                schema: "cache",
                table: "products_cache");

            migrationBuilder.DropIndex(
                name: "ix_orders_public_id",
                schema: "orders",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "ix_order_lines_product_cache_id",
                schema: "orders",
                table: "order_lines");

            migrationBuilder.DropIndex(
                name: "ix_customers_public_id",
                schema: "orders",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "ref_public_id",
                schema: "cache",
                table: "products_cache");

            migrationBuilder.DropColumn(
                name: "public_id",
                schema: "orders",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "details",
                schema: "orders",
                table: "order_lines");

            migrationBuilder.DropColumn(
                name: "line_type",
                schema: "orders",
                table: "order_lines");

            migrationBuilder.DropColumn(
                name: "product_cache_id",
                schema: "orders",
                table: "order_lines");

            migrationBuilder.DropColumn(
                name: "date_of_birth",
                schema: "orders",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "first_name",
                schema: "orders",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "last_name",
                schema: "orders",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "middle_name",
                schema: "orders",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "public_id",
                schema: "orders",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "status",
                schema: "orders",
                table: "customers");

            migrationBuilder.RenameColumn(
                name: "sort_order",
                schema: "orders",
                table: "order_lines",
                newName: "product_id");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                schema: "cache",
                table: "products_cache",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);

            migrationBuilder.AddColumn<string>(
                name: "email",
                schema: "orders",
                table: "customers",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "name",
                schema: "orders",
                table: "customers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_products_cache_is_active",
                schema: "cache",
                table: "products_cache",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_order_lines_product_id",
                schema: "orders",
                table: "order_lines",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_customers_email",
                schema: "orders",
                table: "customers",
                column: "email",
                unique: true);
        }
    }
}
