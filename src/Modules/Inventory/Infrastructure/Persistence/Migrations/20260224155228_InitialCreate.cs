using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Inventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "inventories");

            migrationBuilder.EnsureSchema(
                name: "cache");

            migrationBuilder.EnsureSchema(
                name: "messaging");

            migrationBuilder.CreateSequence<int>(
                name: "inventories_hilo_seq",
                schema: "inventories",
                incrementBy: 10);

            migrationBuilder.CreateTable(
                name: "ancillary_data",
                schema: "inventories",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    ref_home_center_number = table.Column<int>(type: "integer", nullable: false),
                    ref_stock_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    customer_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    package_received_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_synced_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ancillary_data", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                schema: "inventories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    entity_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    action = table.Column<int>(type: "integer", nullable: false),
                    old_values = table.Column<string>(type: "jsonb", nullable: true),
                    new_values = table.Column<string>(type: "jsonb", nullable: true),
                    affected_columns = table.Column<string>(type: "jsonb", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    timestamp_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    correlation_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    trace_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "home_centers_cache",
                schema: "cache",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    ref_home_center_number = table.Column<int>(type: "integer", nullable: false),
                    lot_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    state_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    zone_id = table.Column<int>(type: "integer", nullable: true),
                    region_id = table.Column<int>(type: "integer", nullable: true),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    last_synced_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_home_centers_cache", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inbox_message_consumers",
                schema: "messaging",
                columns: table => new
                {
                    inbox_message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inbox_message_consumers", x => new { x.inbox_message_id, x.name });
                });

            migrationBuilder.CreateTable(
                name: "inbox_messages",
                schema: "messaging",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    occurred_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error = table.Column<string>(type: "text", nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    next_retry_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "land_costs",
                schema: "inventories",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    ref_home_center_number = table.Column<int>(type: "integer", nullable: false),
                    ref_stock_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    add_to_total = table.Column<string>(type: "text", nullable: true),
                    furniture_total = table.Column<string>(type: "text", nullable: true),
                    last_synced_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_land_costs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "land_parcels",
                schema: "inventories",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    ref_home_center_number = table.Column<int>(type: "integer", nullable: false),
                    ref_stock_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    stock_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    land_age = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    land_cost = table.Column<string>(type: "text", nullable: true),
                    add_to_total = table.Column<string>(type: "text", nullable: true),
                    appraisal = table.Column<string>(type: "text", nullable: true),
                    map_parcel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    address2 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    state = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    zip = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    county = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    loan_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    home_stock_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    last_synced_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_land_parcels", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "on_lot_homes",
                schema: "inventories",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    ref_home_center_number = table.Column<int>(type: "integer", nullable: false),
                    ref_stock_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    stock_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    condition = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    build_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    width = table.Column<decimal>(type: "numeric", nullable: true),
                    length = table.Column<decimal>(type: "numeric", nullable: true),
                    number_of_bedrooms = table.Column<int>(type: "integer", nullable: true),
                    number_of_bathrooms = table.Column<int>(type: "integer", nullable: true),
                    model_year = table.Column<int>(type: "integer", nullable: true),
                    model = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    make = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    facility = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    serial_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    total_invoice_amount = table.Column<string>(type: "text", nullable: true),
                    purchase_discount = table.Column<string>(type: "text", nullable: true),
                    original_retail_price = table.Column<string>(type: "text", nullable: true),
                    current_retail_price = table.Column<string>(type: "text", nullable: true),
                    stocked_in_date = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    land_stock_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    last_synced_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_on_lot_homes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_message_consumers",
                schema: "messaging",
                columns: table => new
                {
                    outbox_message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_message_consumers", x => new { x.outbox_message_id, x.name });
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "messaging",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    occurred_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error = table.Column<string>(type: "text", nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    next_retry_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sale_summaries_cache",
                schema: "cache",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    ref_stock_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    sale_id = table.Column<int>(type: "integer", nullable: true),
                    customer_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    received_in_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    original_retail_price = table.Column<string>(type: "text", nullable: true),
                    current_retail_price = table.Column<string>(type: "text", nullable: true),
                    last_synced_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sale_summaries_cache", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "wheels_and_axles_transactions",
                schema: "inventories",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    ref_home_center_number = table.Column<int>(type: "integer", nullable: false),
                    ref_transaction_id = table.Column<int>(type: "integer", nullable: false),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    stock_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    wheels = table.Column<int>(type: "integer", nullable: false),
                    wheel_value = table.Column<string>(type: "text", nullable: false),
                    brake_axles = table.Column<int>(type: "integer", nullable: false),
                    brake_axle_value = table.Column<string>(type: "text", nullable: false),
                    idler_axles = table.Column<int>(type: "integer", nullable: false),
                    idler_axle_value = table.Column<string>(type: "text", nullable: false),
                    total_wheels_and_axles_value = table.Column<string>(type: "text", nullable: false),
                    last_synced_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wheels_and_axles_transactions", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_ancillary_data_hc_stock",
                schema: "inventories",
                table: "ancillary_data",
                columns: new[] { "ref_home_center_number", "ref_stock_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_correlation",
                schema: "inventories",
                table: "audit_logs",
                column: "correlation_id");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_entity",
                schema: "inventories",
                table: "audit_logs",
                columns: new[] { "entity_name", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_timestamp",
                schema: "inventories",
                table: "audit_logs",
                column: "timestamp_utc");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_user",
                schema: "inventories",
                table: "audit_logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_home_centers_cache_ref_home_center_number",
                schema: "cache",
                table: "home_centers_cache",
                column: "ref_home_center_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_inbox_messages_processed_next_retry",
                schema: "messaging",
                table: "inbox_messages",
                columns: new[] { "processed_on_utc", "next_retry_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_land_costs_hc_stock",
                schema: "inventories",
                table: "land_costs",
                columns: new[] { "ref_home_center_number", "ref_stock_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_land_parcels_hc_stock",
                schema: "inventories",
                table: "land_parcels",
                columns: new[] { "ref_home_center_number", "ref_stock_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_land_parcels_ref_home_center_number",
                schema: "inventories",
                table: "land_parcels",
                column: "ref_home_center_number");

            migrationBuilder.CreateIndex(
                name: "ix_on_lot_homes_hc_stock",
                schema: "inventories",
                table: "on_lot_homes",
                columns: new[] { "ref_home_center_number", "ref_stock_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_on_lot_homes_ref_home_center_number",
                schema: "inventories",
                table: "on_lot_homes",
                column: "ref_home_center_number");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_processed_next_retry",
                schema: "messaging",
                table: "outbox_messages",
                columns: new[] { "processed_on_utc", "next_retry_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_sale_summaries_cache_ref_stock_number",
                schema: "cache",
                table: "sale_summaries_cache",
                column: "ref_stock_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_wheels_and_axles_hc_txn",
                schema: "inventories",
                table: "wheels_and_axles_transactions",
                columns: new[] { "ref_home_center_number", "ref_transaction_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_wheels_and_axles_ref_home_center_number",
                schema: "inventories",
                table: "wheels_and_axles_transactions",
                column: "ref_home_center_number");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ancillary_data",
                schema: "inventories");

            migrationBuilder.DropTable(
                name: "audit_logs",
                schema: "inventories");

            migrationBuilder.DropTable(
                name: "home_centers_cache",
                schema: "cache");

            migrationBuilder.DropTable(
                name: "inbox_message_consumers",
                schema: "messaging");

            migrationBuilder.DropTable(
                name: "inbox_messages",
                schema: "messaging");

            migrationBuilder.DropTable(
                name: "land_costs",
                schema: "inventories");

            migrationBuilder.DropTable(
                name: "land_parcels",
                schema: "inventories");

            migrationBuilder.DropTable(
                name: "on_lot_homes",
                schema: "inventories");

            migrationBuilder.DropTable(
                name: "outbox_message_consumers",
                schema: "messaging");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "messaging");

            migrationBuilder.DropTable(
                name: "sale_summaries_cache",
                schema: "cache");

            migrationBuilder.DropTable(
                name: "wheels_and_axles_transactions",
                schema: "inventories");

            migrationBuilder.DropSequence(
                name: "inventories_hilo_seq",
                schema: "inventories");
        }
    }
}
