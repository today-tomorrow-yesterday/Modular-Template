using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System.Text.Json;

#nullable disable

namespace Modules.Sales.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "sales");

            migrationBuilder.EnsureSchema(
                name: "cache");

            migrationBuilder.EnsureSchema(
                name: "messaging");

            migrationBuilder.EnsureSchema(
                name: "packages");

            migrationBuilder.CreateSequence<int>(
                name: "sales_hilo_seq",
                schema: "sales",
                incrementBy: 10);

            migrationBuilder.CreateTable(
                name: "audit_logs",
                schema: "sales",
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
                name: "authorized_users_cache",
                schema: "cache",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    ref_user_id = table.Column<int>(type: "integer", nullable: false),
                    federated_id = table.Column<string>(type: "text", nullable: false),
                    employee_number = table.Column<int>(type: "integer", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    display_name = table.Column<string>(type: "text", nullable: false),
                    email_address = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_retired = table.Column<bool>(type: "boolean", nullable: false),
                    authorized_home_centers = table.Column<int[]>(type: "integer[]", nullable: false),
                    last_synced_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_authorized_users_cache", x => x.id);
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
                name: "land_parcels_cache",
                schema: "cache",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    ref_land_parcel_id = table.Column<int>(type: "integer", nullable: false),
                    ref_home_center_number = table.Column<int>(type: "integer", nullable: false),
                    ref_stock_number = table.Column<string>(type: "text", nullable: false),
                    stock_type = table.Column<string>(type: "text", nullable: true),
                    land_cost = table.Column<string>(type: "text", nullable: true),
                    appraisal = table.Column<string>(type: "text", nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    city = table.Column<string>(type: "text", nullable: true),
                    state = table.Column<string>(type: "text", nullable: true),
                    zip = table.Column<string>(type: "text", nullable: true),
                    county = table.Column<string>(type: "text", nullable: true),
                    last_synced_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_land_parcels_cache", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "on_lot_homes_cache",
                schema: "cache",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    ref_on_lot_home_id = table.Column<int>(type: "integer", nullable: false),
                    ref_home_center_number = table.Column<int>(type: "integer", nullable: false),
                    ref_stock_number = table.Column<string>(type: "text", nullable: false),
                    stock_type = table.Column<string>(type: "text", nullable: true),
                    condition = table.Column<string>(type: "text", nullable: true),
                    build_type = table.Column<string>(type: "text", nullable: true),
                    width = table.Column<decimal>(type: "numeric", nullable: true),
                    length = table.Column<decimal>(type: "numeric", nullable: true),
                    number_of_bedrooms = table.Column<int>(type: "integer", nullable: true),
                    number_of_bathrooms = table.Column<int>(type: "integer", nullable: true),
                    model_year = table.Column<int>(type: "integer", nullable: true),
                    model = table.Column<string>(type: "text", nullable: true),
                    make = table.Column<string>(type: "text", nullable: true),
                    facility = table.Column<string>(type: "text", nullable: true),
                    serial_number = table.Column<string>(type: "text", nullable: true),
                    total_invoice_amount = table.Column<string>(type: "text", nullable: true),
                    original_retail_price = table.Column<string>(type: "text", nullable: true),
                    current_retail_price = table.Column<string>(type: "text", nullable: true),
                    last_synced_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_on_lot_homes_cache", x => x.id);
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
                name: "parties",
                schema: "cache",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    ref_party_id = table.Column<int>(type: "integer", nullable: false),
                    ref_public_id = table.Column<Guid>(type: "uuid", nullable: false),
                    party_type = table.Column<string>(type: "text", nullable: false),
                    lifecycle_stage = table.Column<string>(type: "text", nullable: false),
                    home_center_number = table.Column<int>(type: "integer", nullable: false),
                    display_name = table.Column<string>(type: "text", nullable: false),
                    salesforce_account_id = table.Column<string>(type: "text", nullable: true),
                    last_synced_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_parties", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "retail_locations",
                schema: "sales",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    location_type = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    state_code = table.Column<string>(type: "text", nullable: false),
                    zip = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    ref_home_center_number = table.Column<int>(type: "integer", nullable: true),
                    organization_metadata = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    modified_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_retail_locations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "party_organizations",
                schema: "cache",
                columns: table => new
                {
                    party_id = table.Column<int>(type: "integer", nullable: false),
                    organization_name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_party_organizations", x => x.party_id);
                    table.ForeignKey(
                        name: "fk_party_organizations_parties_party_id",
                        column: x => x.party_id,
                        principalSchema: "cache",
                        principalTable: "parties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "party_persons",
                schema: "cache",
                columns: table => new
                {
                    party_id = table.Column<int>(type: "integer", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    middle_name = table.Column<string>(type: "text", nullable: true),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: true),
                    phone = table.Column<string>(type: "text", nullable: true),
                    co_buyer_first_name = table.Column<string>(type: "text", nullable: true),
                    co_buyer_last_name = table.Column<string>(type: "text", nullable: true),
                    primary_sales_person_federated_id = table.Column<string>(type: "text", nullable: true),
                    primary_sales_person_first_name = table.Column<string>(type: "text", nullable: true),
                    primary_sales_person_last_name = table.Column<string>(type: "text", nullable: true),
                    secondary_sales_person_federated_id = table.Column<string>(type: "text", nullable: true),
                    secondary_sales_person_first_name = table.Column<string>(type: "text", nullable: true),
                    secondary_sales_person_last_name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_party_persons", x => x.party_id);
                    table.ForeignKey(
                        name: "fk_party_persons_parties_party_id",
                        column: x => x.party_id,
                        principalSchema: "cache",
                        principalTable: "parties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sales",
                schema: "sales",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    party_id = table.Column<int>(type: "integer", nullable: false),
                    retail_location_id = table.Column<int>(type: "integer", nullable: false),
                    sale_type = table.Column<string>(type: "text", nullable: false),
                    sale_status = table.Column<string>(type: "text", nullable: false),
                    sale_number = table.Column<int>(type: "integer", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    modified_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sales", x => x.id);
                    table.ForeignKey(
                        name: "fk_sales_parties_cache_party_id",
                        column: x => x.party_id,
                        principalSchema: "cache",
                        principalTable: "parties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_sales_retail_locations_retail_location_id",
                        column: x => x.retail_location_id,
                        principalSchema: "sales",
                        principalTable: "retail_locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "delivery_addresses",
                schema: "sales",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    sale_id = table.Column<int>(type: "integer", nullable: false),
                    address_style = table.Column<string>(type: "text", nullable: true),
                    address_type = table.Column<string>(type: "text", nullable: true),
                    address_line_1 = table.Column<string>(type: "text", nullable: true),
                    address_line_2 = table.Column<string>(type: "text", nullable: true),
                    address_line_3 = table.Column<string>(type: "text", nullable: true),
                    city = table.Column<string>(type: "text", nullable: true),
                    county = table.Column<string>(type: "text", nullable: true),
                    state = table.Column<string>(type: "text", nullable: true),
                    country = table.Column<string>(type: "text", nullable: true),
                    postal_code = table.Column<string>(type: "text", nullable: true),
                    occupancy_type = table.Column<string>(type: "text", nullable: true),
                    is_within_city_limits = table.Column<bool>(type: "boolean", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    modified_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_delivery_addresses", x => x.id);
                    table.ForeignKey(
                        name: "fk_delivery_addresses_sales_sale_id",
                        column: x => x.sale_id,
                        principalSchema: "sales",
                        principalTable: "sales",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "packages",
                schema: "packages",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    sale_id = table.Column<int>(type: "integer", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: true),
                    ranking = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    gross_profit = table.Column<string>(type: "text", nullable: false),
                    commissionable_gross_profit = table.Column<string>(type: "text", nullable: false),
                    must_recalculate_taxes = table.Column<bool>(type: "boolean", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    modified_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_packages", x => x.id);
                    table.ForeignKey(
                        name: "fk_packages_sales_sale_id",
                        column: x => x.sale_id,
                        principalSchema: "sales",
                        principalTable: "sales",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "funding_requests_cache",
                schema: "cache",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    ref_funding_request_id = table.Column<int>(type: "integer", nullable: false),
                    sale_id = table.Column<int>(type: "integer", nullable: false),
                    package_id = table.Column<int>(type: "integer", nullable: false),
                    funding_keys = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    lender_id = table.Column<int>(type: "integer", nullable: false),
                    lender_name = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    approval_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    approval_expiration_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_synced_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_funding_requests_cache", x => x.id);
                    table.ForeignKey(
                        name: "fk_funding_requests_cache_packages_package_id",
                        column: x => x.package_id,
                        principalSchema: "packages",
                        principalTable: "packages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_funding_requests_cache_sales_sale_id",
                        column: x => x.sale_id,
                        principalSchema: "sales",
                        principalTable: "sales",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "package_lines",
                schema: "packages",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    package_id = table.Column<int>(type: "integer", nullable: false),
                    line_type = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    sale_price = table.Column<string>(type: "text", nullable: false),
                    estimated_cost = table.Column<string>(type: "text", nullable: false),
                    retail_sale_price = table.Column<string>(type: "text", nullable: false),
                    responsibility = table.Column<string>(type: "text", nullable: true),
                    should_exclude_from_pricing = table.Column<bool>(type: "boolean", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    credit_details = table.Column<string>(type: "jsonb", nullable: true),
                    on_lot_home_id = table.Column<int>(type: "integer", nullable: true),
                    home_details = table.Column<string>(type: "jsonb", nullable: true),
                    insurance_details = table.Column<string>(type: "jsonb", nullable: true),
                    land_parcel_id = table.Column<int>(type: "integer", nullable: true),
                    land_details = table.Column<string>(type: "jsonb", nullable: true),
                    project_cost_details = table.Column<string>(type: "jsonb", nullable: true),
                    sales_team_details = table.Column<string>(type: "jsonb", nullable: true),
                    tax_details = table.Column<string>(type: "jsonb", nullable: true),
                    trade_in_details = table.Column<string>(type: "jsonb", nullable: true),
                    warranty_details = table.Column<string>(type: "jsonb", nullable: true),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    modified_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_package_lines", x => x.id);
                    table.ForeignKey(
                        name: "fk_package_lines_land_parcels_cache_land_parcel_id",
                        column: x => x.land_parcel_id,
                        principalSchema: "cache",
                        principalTable: "land_parcels_cache",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_package_lines_on_lot_homes_cache_on_lot_home_id",
                        column: x => x.on_lot_home_id,
                        principalSchema: "cache",
                        principalTable: "on_lot_homes_cache",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_package_lines_packages_package_id",
                        column: x => x.package_id,
                        principalSchema: "packages",
                        principalTable: "packages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_correlation",
                schema: "sales",
                table: "audit_logs",
                column: "correlation_id");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_entity",
                schema: "sales",
                table: "audit_logs",
                columns: new[] { "entity_name", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_timestamp",
                schema: "sales",
                table: "audit_logs",
                column: "timestamp_utc");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_user",
                schema: "sales",
                table: "audit_logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_authorized_users_cache_federated_id",
                schema: "cache",
                table: "authorized_users_cache",
                column: "federated_id",
                unique: true,
                filter: "federated_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_authorized_users_cache_ref_user_id",
                schema: "cache",
                table: "authorized_users_cache",
                column: "ref_user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_delivery_addresses_public_id",
                schema: "sales",
                table: "delivery_addresses",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_delivery_addresses_sale_id",
                schema: "sales",
                table: "delivery_addresses",
                column: "sale_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_funding_requests_cache_package_id",
                schema: "cache",
                table: "funding_requests_cache",
                column: "package_id");

            migrationBuilder.CreateIndex(
                name: "ix_funding_requests_cache_ref_funding_request_id",
                schema: "cache",
                table: "funding_requests_cache",
                column: "ref_funding_request_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_funding_requests_cache_sale_id",
                schema: "cache",
                table: "funding_requests_cache",
                column: "sale_id");

            migrationBuilder.CreateIndex(
                name: "ix_inbox_messages_processed_next_retry",
                schema: "messaging",
                table: "inbox_messages",
                columns: new[] { "processed_on_utc", "next_retry_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_land_parcels_cache_hc_stock",
                schema: "cache",
                table: "land_parcels_cache",
                columns: new[] { "ref_home_center_number", "ref_stock_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_land_parcels_cache_ref_land_parcel_id",
                schema: "cache",
                table: "land_parcels_cache",
                column: "ref_land_parcel_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_on_lot_homes_cache_hc_stock",
                schema: "cache",
                table: "on_lot_homes_cache",
                columns: new[] { "ref_home_center_number", "ref_stock_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_on_lot_homes_cache_ref_on_lot_home_id",
                schema: "cache",
                table: "on_lot_homes_cache",
                column: "ref_on_lot_home_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_processed_next_retry",
                schema: "messaging",
                table: "outbox_messages",
                columns: new[] { "processed_on_utc", "next_retry_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_package_lines_land_parcel_id",
                schema: "packages",
                table: "package_lines",
                column: "land_parcel_id",
                filter: "land_parcel_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_package_lines_on_lot_home_id",
                schema: "packages",
                table: "package_lines",
                column: "on_lot_home_id",
                filter: "on_lot_home_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_package_lines_package_id",
                schema: "packages",
                table: "package_lines",
                column: "package_id");

            migrationBuilder.CreateIndex(
                name: "ix_packages_public_id",
                schema: "packages",
                table: "packages",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_packages_sale_id",
                schema: "packages",
                table: "packages",
                column: "sale_id");

            migrationBuilder.CreateIndex(
                name: "ix_parties_home_center_number",
                schema: "cache",
                table: "parties",
                column: "home_center_number");

            migrationBuilder.CreateIndex(
                name: "ix_parties_ref_party_id",
                schema: "cache",
                table: "parties",
                column: "ref_party_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_parties_ref_public_id",
                schema: "cache",
                table: "parties",
                column: "ref_public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_party_persons_primary_sp_federated_id",
                schema: "cache",
                table: "party_persons",
                column: "primary_sales_person_federated_id");

            migrationBuilder.CreateIndex(
                name: "ix_retail_locations_ref_home_center_number",
                schema: "sales",
                table: "retail_locations",
                column: "ref_home_center_number",
                unique: true,
                filter: "ref_home_center_number IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_sales_party_id",
                schema: "sales",
                table: "sales",
                column: "party_id");

            migrationBuilder.CreateIndex(
                name: "ix_sales_public_id",
                schema: "sales",
                table: "sales",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sales_retail_location_id",
                schema: "sales",
                table: "sales",
                column: "retail_location_id");

            migrationBuilder.CreateIndex(
                name: "ix_sales_sale_number",
                schema: "sales",
                table: "sales",
                column: "sale_number");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "authorized_users_cache",
                schema: "cache");

            migrationBuilder.DropTable(
                name: "delivery_addresses",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "funding_requests_cache",
                schema: "cache");

            migrationBuilder.DropTable(
                name: "inbox_message_consumers",
                schema: "messaging");

            migrationBuilder.DropTable(
                name: "inbox_messages",
                schema: "messaging");

            migrationBuilder.DropTable(
                name: "outbox_message_consumers",
                schema: "messaging");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "messaging");

            migrationBuilder.DropTable(
                name: "package_lines",
                schema: "packages");

            migrationBuilder.DropTable(
                name: "party_organizations",
                schema: "cache");

            migrationBuilder.DropTable(
                name: "party_persons",
                schema: "cache");

            migrationBuilder.DropTable(
                name: "land_parcels_cache",
                schema: "cache");

            migrationBuilder.DropTable(
                name: "on_lot_homes_cache",
                schema: "cache");

            migrationBuilder.DropTable(
                name: "packages",
                schema: "packages");

            migrationBuilder.DropTable(
                name: "sales",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "parties",
                schema: "cache");

            migrationBuilder.DropTable(
                name: "retail_locations",
                schema: "sales");

            migrationBuilder.DropSequence(
                name: "sales_hilo_seq",
                schema: "sales");
        }
    }
}
