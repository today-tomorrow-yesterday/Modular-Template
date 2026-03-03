using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Sales.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRefPartyIdFromPartiesCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_parties_ref_party_id",
                schema: "cache",
                table: "parties");

            migrationBuilder.DropColumn(
                name: "ref_party_id",
                schema: "cache",
                table: "parties");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ref_party_id",
                schema: "cache",
                table: "parties",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_parties_ref_party_id",
                schema: "cache",
                table: "parties",
                column: "ref_party_id",
                unique: true);
        }
    }
}
