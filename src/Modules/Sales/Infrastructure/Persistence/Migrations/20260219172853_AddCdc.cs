using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Sales.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCdc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "cdc");

            migrationBuilder.CreateTable(
                name: "pricing_home_multiplier",
                schema: "cdc",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    homident = table.Column<int>(type: "integer", nullable: false),
                    state_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    effective_date = table.Column<DateOnly>(type: "date", nullable: false),
                    home_multiplier_value = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    freight_multiplier = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    upgrades_multiplier = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    wheels_axles_multiplier = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    dues_multiplier = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pricing_home_multiplier", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pricing_home_option_whitelist",
                schema: "cdc",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    howident = table.Column<int>(type: "integer", nullable: false),
                    plant_number = table.Column<int>(type: "integer", nullable: false),
                    option_number = table.Column<int>(type: "integer", nullable: false),
                    multiplier_code = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    effective_date = table.Column<DateOnly>(type: "date", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pricing_home_option_whitelist", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "project_cost_category",
                schema: "cdc",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    master_dealer = table.Column<int>(type: "integer", nullable: false),
                    category_number = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    is_credit_consideration = table.Column<bool>(type: "boolean", nullable: false),
                    is_land_dot = table.Column<bool>(type: "boolean", nullable: false),
                    restrict_fha = table.Column<bool>(type: "boolean", nullable: false),
                    restrict_css = table.Column<bool>(type: "boolean", nullable: false),
                    display_for_cash = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project_cost_category", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tax_allowance_position",
                schema: "cdc",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    type_code = table.Column<string>(type: "text", nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    cost_gl_clayton = table.Column<int>(type: "integer", nullable: false),
                    sale_gl_clayton = table.Column<int>(type: "integer", nullable: false),
                    cost_gl_global = table.Column<string>(type: "text", nullable: false),
                    sale_gl_global = table.Column<string>(type: "text", nullable: false),
                    is_mandatory = table.Column<bool>(type: "boolean", nullable: false),
                    is_mandatory_sale = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tax_allowance_position", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tax_calculation_error",
                schema: "cdc",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    funding_cache_id = table.Column<int>(type: "integer", nullable: false),
                    link_id = table.Column<int>(type: "integer", nullable: true),
                    sequence_number = table.Column<int>(type: "integer", nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    master_dealer = table.Column<int>(type: "integer", nullable: false),
                    home_center_number = table.Column<int>(type: "integer", nullable: false),
                    field_name = table.Column<string>(type: "text", nullable: false),
                    message_id = table.Column<string>(type: "text", nullable: false),
                    program_name = table.Column<string>(type: "text", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tax_calculation_error", x => x.id);
                    table.ForeignKey(
                        name: "fk_tax_calculation_error_funding_requests_cache_funding_cache_",
                        column: x => x.funding_cache_id,
                        principalSchema: "cache",
                        principalTable: "funding_requests_cache",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tax_exemption",
                schema: "cdc",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    exemption_code = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    rules_text = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tax_exemption", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tax_question",
                schema: "cdc",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    master_dealer = table.Column<int>(type: "integer", nullable: false),
                    state_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    question_number = table.Column<int>(type: "integer", nullable: false),
                    effective_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    ask_for_new = table.Column<bool>(type: "boolean", nullable: false),
                    ask_for_used = table.Column<bool>(type: "boolean", nullable: false),
                    ask_for_repo = table.Column<bool>(type: "boolean", nullable: false),
                    ask_for_land = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tax_question", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "project_cost_item",
                schema: "cdc",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    master_dealer = table.Column<int>(type: "integer", nullable: false),
                    project_cost_category_id = table.Column<int>(type: "integer", nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    item_number = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    is_fee_item = table.Column<bool>(type: "boolean", nullable: false),
                    is_css_restricted = table.Column<bool>(type: "boolean", nullable: false),
                    is_fha_restricted = table.Column<bool>(type: "boolean", nullable: false),
                    is_display_for_cash = table.Column<bool>(type: "boolean", nullable: false),
                    is_restrict_option_price = table.Column<bool>(type: "boolean", nullable: false),
                    is_restrict_css_cost = table.Column<bool>(type: "boolean", nullable: false),
                    is_hope_refunds_included = table.Column<bool>(type: "boolean", nullable: false),
                    profit_percentage = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project_cost_item", x => x.id);
                    table.ForeignKey(
                        name: "fk_project_cost_item_project_cost_category_project_cost_catego",
                        column: x => x.project_cost_category_id,
                        principalSchema: "cdc",
                        principalTable: "project_cost_category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tax_question_text",
                schema: "cdc",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    tax_question_id = table.Column<int>(type: "integer", nullable: false),
                    question_number = table.Column<int>(type: "integer", nullable: false),
                    text = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    inactivate_date = table.Column<DateOnly>(type: "date", nullable: true),
                    inactivated_by = table.Column<string>(type: "text", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tax_question_text", x => x.id);
                    table.ForeignKey(
                        name: "fk_tax_question_text_tax_question_tax_question_id",
                        column: x => x.tax_question_id,
                        principalSchema: "cdc",
                        principalTable: "tax_question",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_cost_state_matrix",
                schema: "cdc",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    master_dealer = table.Column<int>(type: "integer", nullable: false),
                    project_cost_category_id = table.Column<int>(type: "integer", nullable: false),
                    project_cost_item_id = table.Column<int>(type: "integer", nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    category_item_id = table.Column<int>(type: "integer", nullable: false),
                    home_type = table.Column<string>(type: "text", nullable: false),
                    state_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    tax_basis_manufactured = table.Column<decimal>(type: "numeric(4,3)", precision: 4, scale: 3, nullable: false),
                    tax_basis_modular_on = table.Column<decimal>(type: "numeric(4,3)", precision: 4, scale: 3, nullable: false),
                    tax_basis_modular_off = table.Column<decimal>(type: "numeric(4,3)", precision: 4, scale: 3, nullable: false),
                    is_insurable = table.Column<bool>(type: "boolean", nullable: false),
                    is_adj_struct_insurable = table.Column<bool>(type: "boolean", nullable: true),
                    is_total_improvement_included = table.Column<bool>(type: "boolean", nullable: true),
                    is_fee_item_allowed = table.Column<bool>(type: "boolean", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project_cost_state_matrix", x => x.id);
                    table.ForeignKey(
                        name: "fk_project_cost_state_matrix_project_cost_category_project_cos",
                        column: x => x.project_cost_category_id,
                        principalSchema: "cdc",
                        principalTable: "project_cost_category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_project_cost_state_matrix_project_cost_item_project_cost_it",
                        column: x => x.project_cost_item_id,
                        principalSchema: "cdc",
                        principalTable: "project_cost_item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "uq_cdc_pricing_home_multiplier_homident",
                schema: "cdc",
                table: "pricing_home_multiplier",
                column: "homident",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_cdc_pricing_home_option_whitelist_howident",
                schema: "cdc",
                table: "pricing_home_option_whitelist",
                column: "howident",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_cdc_project_cost_category_dealer_number",
                schema: "cdc",
                table: "project_cost_category",
                columns: new[] { "master_dealer", "category_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_project_cost_item_project_cost_category_id",
                schema: "cdc",
                table: "project_cost_item",
                column: "project_cost_category_id");

            migrationBuilder.CreateIndex(
                name: "uq_cdc_project_cost_item_dealer_cat_number",
                schema: "cdc",
                table: "project_cost_item",
                columns: new[] { "master_dealer", "category_id", "item_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_project_cost_state_matrix_project_cost_category_id",
                schema: "cdc",
                table: "project_cost_state_matrix",
                column: "project_cost_category_id");

            migrationBuilder.CreateIndex(
                name: "ix_project_cost_state_matrix_project_cost_item_id",
                schema: "cdc",
                table: "project_cost_state_matrix",
                column: "project_cost_item_id");

            migrationBuilder.CreateIndex(
                name: "uq_cdc_project_cost_state_matrix_composite",
                schema: "cdc",
                table: "project_cost_state_matrix",
                columns: new[] { "master_dealer", "category_id", "category_item_id", "home_type", "state_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_cdc_tax_allowance_position",
                schema: "cdc",
                table: "tax_allowance_position",
                column: "position",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_cdc_tax_calc_error_funding_seq",
                schema: "cdc",
                table: "tax_calculation_error",
                columns: new[] { "funding_cache_id", "sequence_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_cdc_tax_exemption_code",
                schema: "cdc",
                table: "tax_exemption",
                column: "exemption_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_cdc_tax_question_dealer_state_number",
                schema: "cdc",
                table: "tax_question",
                columns: new[] { "master_dealer", "state_code", "question_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tax_question_text_tax_question_id",
                schema: "cdc",
                table: "tax_question_text",
                column: "tax_question_id");

            migrationBuilder.CreateIndex(
                name: "uq_cdc_tax_question_text_number",
                schema: "cdc",
                table: "tax_question_text",
                column: "question_number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pricing_home_multiplier",
                schema: "cdc");

            migrationBuilder.DropTable(
                name: "pricing_home_option_whitelist",
                schema: "cdc");

            migrationBuilder.DropTable(
                name: "project_cost_state_matrix",
                schema: "cdc");

            migrationBuilder.DropTable(
                name: "tax_allowance_position",
                schema: "cdc");

            migrationBuilder.DropTable(
                name: "tax_calculation_error",
                schema: "cdc");

            migrationBuilder.DropTable(
                name: "tax_exemption",
                schema: "cdc");

            migrationBuilder.DropTable(
                name: "tax_question_text",
                schema: "cdc");

            migrationBuilder.DropTable(
                name: "project_cost_item",
                schema: "cdc");

            migrationBuilder.DropTable(
                name: "tax_question",
                schema: "cdc");

            migrationBuilder.DropTable(
                name: "project_cost_category",
                schema: "cdc");
        }
    }
}
