using Microsoft.EntityFrameworkCore.Migrations;
using Modules.Funding.Domain.FundingRequests;

#nullable disable

namespace Modules.Funding.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "fundings");

            migrationBuilder.EnsureSchema(
                name: "cache");

            migrationBuilder.EnsureSchema(
                name: "messaging");

            migrationBuilder.CreateSequence<int>(
                name: "fundings_hilo_seq",
                schema: "fundings",
                incrementBy: 10);

            migrationBuilder.CreateTable(
                name: "audit_logs",
                schema: "fundings",
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
                name: "customers_cache",
                schema: "cache",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    loan_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    first_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    last_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    home_center_number = table.Column<int>(type: "integer", nullable: false),
                    last_synced_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customers_cache", x => x.id);
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
                name: "pending_funding_requests",
                schema: "fundings",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    loan_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    sale_id = table.Column<int>(type: "integer", nullable: false),
                    package_id = table.Column<int>(type: "integer", nullable: false),
                    ref_customer_id = table.Column<int>(type: "integer", nullable: true),
                    request_amount = table.Column<string>(type: "text", nullable: false),
                    home_center_number = table.Column<int>(type: "integer", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    funding_keys = table.Column<IReadOnlyCollection<FundingKey>>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pending_funding_requests", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "funding_requests",
                schema: "fundings",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    sale_id = table.Column<int>(type: "integer", nullable: false),
                    package_id = table.Column<int>(type: "integer", nullable: false),
                    customer_id = table.Column<int>(type: "integer", nullable: true),
                    ref_customer_id = table.Column<int>(type: "integer", nullable: true),
                    request_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    request_amount = table.Column<string>(type: "text", nullable: false),
                    approval_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    approval_expiration_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lender_id = table.Column<int>(type: "integer", nullable: false),
                    lender_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    home_center_number = table.Column<int>(type: "integer", nullable: true),
                    funding_keys = table.Column<IReadOnlyCollection<FundingKey>>(type: "jsonb", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    modified_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_funding_requests", x => x.id);
                    table.ForeignKey(
                        name: "fk_funding_requests_customers_cache_customer_id",
                        column: x => x.customer_id,
                        principalSchema: "cache",
                        principalTable: "customers_cache",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_correlation",
                schema: "fundings",
                table: "audit_logs",
                column: "correlation_id");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_entity",
                schema: "fundings",
                table: "audit_logs",
                columns: new[] { "entity_name", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_timestamp",
                schema: "fundings",
                table: "audit_logs",
                column: "timestamp_utc");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_user",
                schema: "fundings",
                table: "audit_logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_customers_cache_loan_id",
                schema: "cache",
                table: "customers_cache",
                column: "loan_id");

            migrationBuilder.CreateIndex(
                name: "ix_funding_requests_customer_id",
                schema: "fundings",
                table: "funding_requests",
                column: "customer_id",
                filter: "customer_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_funding_requests_ref_customer_id",
                schema: "fundings",
                table: "funding_requests",
                column: "ref_customer_id",
                filter: "ref_customer_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_funding_requests_sale_id",
                schema: "fundings",
                table: "funding_requests",
                column: "sale_id");

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
                name: "ix_pending_funding_requests_loan_id",
                schema: "fundings",
                table: "pending_funding_requests",
                column: "loan_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs",
                schema: "fundings");

            migrationBuilder.DropTable(
                name: "funding_requests",
                schema: "fundings");

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
                name: "pending_funding_requests",
                schema: "fundings");

            migrationBuilder.DropTable(
                name: "customers_cache",
                schema: "cache");

            migrationBuilder.DropSequence(
                name: "fundings_hilo_seq",
                schema: "fundings");
        }
    }
}
