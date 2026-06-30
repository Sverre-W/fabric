using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabric.Server.Reception.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReceptionKiosksAndArrivalActors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "offboarded_by_display_name",
                schema: "reception",
                table: "expected_arrivals",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "offboarded_by_identifier",
                schema: "reception",
                table: "expected_arrivals",
                type: "character varying(320)",
                maxLength: 320,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "offboarded_by_type",
                schema: "reception",
                table: "expected_arrivals",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "onboarded_by_display_name",
                schema: "reception",
                table: "expected_arrivals",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "onboarded_by_identifier",
                schema: "reception",
                table: "expected_arrivals",
                type: "character varying(320)",
                maxLength: 320,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "onboarded_by_type",
                schema: "reception",
                table: "expected_arrivals",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "actor_display_name",
                schema: "reception",
                table: "arrival_entries",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "actor_identifier",
                schema: "reception",
                table: "arrival_entries",
                type: "character varying(320)",
                maxLength: 320,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "actor_type",
                schema: "reception",
                table: "arrival_entries",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "reception_kiosks",
                schema: "reception",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    location_id = table.Column<Guid>(type: "uuid", nullable: false),
                    api_key_hash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    api_key_salt = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    enabled = table.Column<bool>(type: "boolean", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reception_kiosks", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_reception_kiosks_tenant_id",
                schema: "reception",
                table: "reception_kiosks",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_reception_kiosks_tenant_id_location_id",
                schema: "reception",
                table: "reception_kiosks",
                columns: new[] { "tenant_id", "location_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reception_kiosks",
                schema: "reception");

            migrationBuilder.DropColumn(
                name: "offboarded_by_display_name",
                schema: "reception",
                table: "expected_arrivals");

            migrationBuilder.DropColumn(
                name: "offboarded_by_identifier",
                schema: "reception",
                table: "expected_arrivals");

            migrationBuilder.DropColumn(
                name: "offboarded_by_type",
                schema: "reception",
                table: "expected_arrivals");

            migrationBuilder.DropColumn(
                name: "onboarded_by_display_name",
                schema: "reception",
                table: "expected_arrivals");

            migrationBuilder.DropColumn(
                name: "onboarded_by_identifier",
                schema: "reception",
                table: "expected_arrivals");

            migrationBuilder.DropColumn(
                name: "onboarded_by_type",
                schema: "reception",
                table: "expected_arrivals");

            migrationBuilder.DropColumn(
                name: "actor_display_name",
                schema: "reception",
                table: "arrival_entries");

            migrationBuilder.DropColumn(
                name: "actor_identifier",
                schema: "reception",
                table: "arrival_entries");

            migrationBuilder.DropColumn(
                name: "actor_type",
                schema: "reception",
                table: "arrival_entries");
        }
    }
}
