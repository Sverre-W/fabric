using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabric.Server.Kiosk.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddKioskDevices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "devices",
                schema: "kiosk",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kiosk_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    agent_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    device_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    enabled = table.Column<bool>(type: "boolean", nullable: false),
                    cleanup_on_session_end = table.Column<bool>(type: "boolean", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_kiosk_devices", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_devices_tenant_id",
                schema: "kiosk",
                table: "devices",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_kiosk_devices_tenant_kiosk_name",
                schema: "kiosk",
                table: "devices",
                columns: new[] { "tenant_id", "kiosk_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_kiosk_devices_tenant_kiosk_type",
                schema: "kiosk",
                table: "devices",
                columns: new[] { "tenant_id", "kiosk_id", "type" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "devices",
                schema: "kiosk");
        }
    }
}
