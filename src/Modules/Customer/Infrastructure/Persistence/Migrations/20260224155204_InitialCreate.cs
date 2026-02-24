using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Modules.Customer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "customers");

            migrationBuilder.EnsureSchema(
                name: "messaging");

            migrationBuilder.CreateSequence<int>(
                name: "customers_hilo_seq",
                schema: "customers",
                incrementBy: 10);

            migrationBuilder.CreateTable(
                name: "audit_logs",
                schema: "customers",
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
                schema: "customers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false),
                    party_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    lifecycle_stage = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    home_center_number = table.Column<int>(type: "integer", nullable: false),
                    salesforce_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    mailing_address_line1 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    mailing_address_line2 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    mailing_city = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    mailing_county = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    mailing_state = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    mailing_country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    mailing_postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    source_created_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    source_last_modified_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_synced_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    organization_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    first_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    middle_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    last_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    name_extension = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    date_of_birth = table.Column<string>(type: "text", nullable: true),
                    co_buyer_party_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_parties", x => x.id);
                    table.ForeignKey(
                        name: "fk_parties_parties_co_buyer_party_id",
                        column: x => x.co_buyer_party_id,
                        principalSchema: "customers",
                        principalTable: "parties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "salespersons",
                schema: "customers",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    username = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    first_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    last_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    lot_number = table.Column<int>(type: "integer", nullable: true),
                    federated_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    last_synced_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_salespersons", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "contact_points",
                schema: "customers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    party_id = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contact_points", x => x.id);
                    table.ForeignKey(
                        name: "fk_contact_points_parties_party_id",
                        column: x => x.party_id,
                        principalSchema: "customers",
                        principalTable: "parties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "party_identifiers",
                schema: "customers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    party_id = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    value = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_party_identifiers", x => x.id);
                    table.ForeignKey(
                        name: "fk_party_identifiers_parties_party_id",
                        column: x => x.party_id,
                        principalSchema: "customers",
                        principalTable: "parties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sales_assignments",
                schema: "customers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    person_id = table.Column<int>(type: "integer", nullable: false),
                    sales_person_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sales_assignments", x => x.id);
                    table.ForeignKey(
                        name: "fk_sales_assignments_persons_person_id",
                        column: x => x.person_id,
                        principalSchema: "customers",
                        principalTable: "parties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_sales_assignments_sales_persons_sales_person_id",
                        column: x => x.sales_person_id,
                        principalSchema: "customers",
                        principalTable: "salespersons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_correlation",
                schema: "customers",
                table: "audit_logs",
                column: "correlation_id");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_entity",
                schema: "customers",
                table: "audit_logs",
                columns: new[] { "entity_name", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_timestamp",
                schema: "customers",
                table: "audit_logs",
                column: "timestamp_utc");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_user",
                schema: "customers",
                table: "audit_logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_contact_points_party_id_type",
                schema: "customers",
                table: "contact_points",
                columns: new[] { "party_id", "type" });

            migrationBuilder.CreateIndex(
                name: "ix_inbox_messages_processed_next_retry",
                schema: "messaging",
                table: "inbox_messages",
                columns: new[] { "processed_on_utc", "next_retry_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_processed_next_retry",
                schema: "messaging",
                table: "outbox_messages",
                columns: new[] { "processed_on_utc", "next_retry_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_parties_co_buyer_party_id",
                schema: "customers",
                table: "parties",
                column: "co_buyer_party_id");

            migrationBuilder.CreateIndex(
                name: "ix_parties_home_center_number",
                schema: "customers",
                table: "parties",
                column: "home_center_number");

            migrationBuilder.CreateIndex(
                name: "ix_parties_lifecycle_stage",
                schema: "customers",
                table: "parties",
                column: "lifecycle_stage");

            migrationBuilder.CreateIndex(
                name: "uq_parties_public_id",
                schema: "customers",
                table: "parties",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_party_identifiers_type_value",
                schema: "customers",
                table: "party_identifiers",
                columns: new[] { "type", "value" });

            migrationBuilder.CreateIndex(
                name: "uq_party_identifiers_party_id_type",
                schema: "customers",
                table: "party_identifiers",
                columns: new[] { "party_id", "type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sales_assignments_person_id",
                schema: "customers",
                table: "sales_assignments",
                column: "person_id",
                unique: true,
                filter: "role = 'Primary'");

            migrationBuilder.CreateIndex(
                name: "ix_sales_assignments_person_id_sales_person_id",
                schema: "customers",
                table: "sales_assignments",
                columns: new[] { "person_id", "sales_person_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sales_assignments_sales_person_id",
                schema: "customers",
                table: "sales_assignments",
                column: "sales_person_id");

            migrationBuilder.CreateIndex(
                name: "uq_salespersons_federated_id",
                schema: "customers",
                table: "salespersons",
                column: "federated_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs",
                schema: "customers");

            migrationBuilder.DropTable(
                name: "contact_points",
                schema: "customers");

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
                name: "party_identifiers",
                schema: "customers");

            migrationBuilder.DropTable(
                name: "sales_assignments",
                schema: "customers");

            migrationBuilder.DropTable(
                name: "parties",
                schema: "customers");

            migrationBuilder.DropTable(
                name: "salespersons",
                schema: "customers");

            migrationBuilder.DropSequence(
                name: "customers_hilo_seq",
                schema: "customers");
        }
    }
}
