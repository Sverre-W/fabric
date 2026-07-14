using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabric.Server.Reception.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RelaxArrivalCodeUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_expected_arrivals_tenant_id_arrival_code",
                schema: "reception",
                table: "expected_arrivals");

            migrationBuilder.CreateIndex(
                name: "ix_expected_arrivals_tenant_id_arrival_code",
                schema: "reception",
                table: "expected_arrivals",
                columns: new[] { "tenant_id", "arrival_code" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_expected_arrivals_tenant_id_arrival_code",
                schema: "reception",
                table: "expected_arrivals");

            migrationBuilder.CreateIndex(
                name: "ix_expected_arrivals_tenant_id_arrival_code",
                schema: "reception",
                table: "expected_arrivals",
                columns: new[] { "tenant_id", "arrival_code" },
                unique: true);
        }
    }
}
