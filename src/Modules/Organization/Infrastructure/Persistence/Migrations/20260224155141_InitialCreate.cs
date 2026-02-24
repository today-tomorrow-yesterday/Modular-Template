using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Organization.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "organizations");

            migrationBuilder.EnsureSchema(
                name: "messaging");

            migrationBuilder.CreateSequence<int>(
                name: "organizations_hilo_seq",
                schema: "organizations",
                incrementBy: 10);

            migrationBuilder.CreateTable(
                name: "audit_logs",
                schema: "organizations",
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
                name: "users",
                schema: "organizations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    ref_user_id = table.Column<int>(type: "integer", nullable: false),
                    first_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    last_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    middle_initial = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    display_name = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    user_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email_address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    employee_number = table.Column<int>(type: "integer", nullable: true),
                    federated_id = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    distinguished_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    user_account_control = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    level1 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    level2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    level3 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    level4 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    position_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    user_roles = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_retired = table.Column<bool>(type: "boolean", nullable: false),
                    last_synced_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "zones",
                schema: "organizations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    ref_zone_id = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    manager = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    status_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    last_synced_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_zones", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "manual_zone_assignments",
                schema: "organizations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    ref_assignment_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    zone = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    status_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    manager = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    last_synced_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_manual_zone_assignments", x => x.id);
                    table.ForeignKey(
                        name: "fk_manual_zone_assignments_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "organizations",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "regions",
                schema: "organizations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    ref_region_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    manager = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    dummy_home_center_number = table.Column<int>(type: "integer", nullable: true),
                    status_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    zone_id = table.Column<int>(type: "integer", nullable: true),
                    last_synced_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_regions", x => x.id);
                    table.ForeignKey(
                        name: "fk_regions_zones_zone_id",
                        column: x => x.zone_id,
                        principalSchema: "organizations",
                        principalTable: "zones",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "home_centers",
                schema: "organizations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    ref_home_center_number = table.Column<int>(type: "integer", nullable: false),
                    lot_mdlr = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    lot_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    lot_dba = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    lot_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    address1 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    address2 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    state_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    zip = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    mailing_address1 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    mailing_address2 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    mailing_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    mailing_state_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    mailing_zip = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    zone_id = table.Column<int>(type: "integer", nullable: true),
                    region_id = table.Column<int>(type: "integer", nullable: true),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    area_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    manager_employee_number = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    last_synced_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_home_centers", x => x.id);
                    table.ForeignKey(
                        name: "fk_home_centers_regions_region_id",
                        column: x => x.region_id,
                        principalSchema: "organizations",
                        principalTable: "regions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_home_centers_zones_zone_id",
                        column: x => x.zone_id,
                        principalSchema: "organizations",
                        principalTable: "zones",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "manual_hc_assignments",
                schema: "organizations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    ref_assignment_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    home_center_id = table.Column<int>(type: "integer", nullable: false),
                    last_synced_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_manual_hc_assignments", x => x.id);
                    table.ForeignKey(
                        name: "fk_manual_hc_assignments_home_centers_home_center_id",
                        column: x => x.home_center_id,
                        principalSchema: "organizations",
                        principalTable: "home_centers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_manual_hc_assignments_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "organizations",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_home_centers",
                schema: "organizations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    home_center_id = table.Column<int>(type: "integer", nullable: false),
                    assignment_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    assigned_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_synced_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_home_centers", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_home_centers_home_centers_home_center_id",
                        column: x => x.home_center_id,
                        principalSchema: "organizations",
                        principalTable: "home_centers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_user_home_centers_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "organizations",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_correlation",
                schema: "organizations",
                table: "audit_logs",
                column: "correlation_id");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_entity",
                schema: "organizations",
                table: "audit_logs",
                columns: new[] { "entity_name", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_timestamp",
                schema: "organizations",
                table: "audit_logs",
                column: "timestamp_utc");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_user",
                schema: "organizations",
                table: "audit_logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_home_centers_ref_home_center_number",
                schema: "organizations",
                table: "home_centers",
                column: "ref_home_center_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_home_centers_region_id",
                schema: "organizations",
                table: "home_centers",
                column: "region_id");

            migrationBuilder.CreateIndex(
                name: "ix_home_centers_zone_id",
                schema: "organizations",
                table: "home_centers",
                column: "zone_id");

            migrationBuilder.CreateIndex(
                name: "ix_inbox_messages_processed_next_retry",
                schema: "messaging",
                table: "inbox_messages",
                columns: new[] { "processed_on_utc", "next_retry_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_manual_hc_assignments_home_center_id",
                schema: "organizations",
                table: "manual_hc_assignments",
                column: "home_center_id");

            migrationBuilder.CreateIndex(
                name: "ix_manual_hc_assignments_ref_assignment_id",
                schema: "organizations",
                table: "manual_hc_assignments",
                column: "ref_assignment_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_manual_hc_assignments_user_id",
                schema: "organizations",
                table: "manual_hc_assignments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_manual_zone_assignments_ref_assignment_id",
                schema: "organizations",
                table: "manual_zone_assignments",
                column: "ref_assignment_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_manual_zone_assignments_user_id",
                schema: "organizations",
                table: "manual_zone_assignments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_processed_next_retry",
                schema: "messaging",
                table: "outbox_messages",
                columns: new[] { "processed_on_utc", "next_retry_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_regions_ref_region_id",
                schema: "organizations",
                table: "regions",
                column: "ref_region_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_regions_zone_id",
                schema: "organizations",
                table: "regions",
                column: "zone_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_home_centers_home_center_id",
                schema: "organizations",
                table: "user_home_centers",
                column: "home_center_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_home_centers_user_id_home_center_id",
                schema: "organizations",
                table: "user_home_centers",
                columns: new[] { "user_id", "home_center_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_employee_number",
                schema: "organizations",
                table: "users",
                column: "employee_number",
                filter: "employee_number IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_users_federated_id",
                schema: "organizations",
                table: "users",
                column: "federated_id",
                unique: true,
                filter: "federated_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_users_ref_user_id",
                schema: "organizations",
                table: "users",
                column: "ref_user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_zones_ref_zone_id",
                schema: "organizations",
                table: "zones",
                column: "ref_zone_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs",
                schema: "organizations");

            migrationBuilder.DropTable(
                name: "inbox_message_consumers",
                schema: "messaging");

            migrationBuilder.DropTable(
                name: "inbox_messages",
                schema: "messaging");

            migrationBuilder.DropTable(
                name: "manual_hc_assignments",
                schema: "organizations");

            migrationBuilder.DropTable(
                name: "manual_zone_assignments",
                schema: "organizations");

            migrationBuilder.DropTable(
                name: "outbox_message_consumers",
                schema: "messaging");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "messaging");

            migrationBuilder.DropTable(
                name: "user_home_centers",
                schema: "organizations");

            migrationBuilder.DropTable(
                name: "home_centers",
                schema: "organizations");

            migrationBuilder.DropTable(
                name: "users",
                schema: "organizations");

            migrationBuilder.DropTable(
                name: "regions",
                schema: "organizations");

            migrationBuilder.DropTable(
                name: "zones",
                schema: "organizations");

            migrationBuilder.DropSequence(
                name: "organizations_hilo_seq",
                schema: "organizations");
        }
    }
}
