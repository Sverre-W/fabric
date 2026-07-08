using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabric.Server.Kiosk.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddKioskDeviceSlots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_kiosk_devices_tenant_kiosk_name",
                schema: "kiosk",
                table: "devices");

            migrationBuilder.DropIndex(
                name: "ix_kiosk_devices_tenant_kiosk_type",
                schema: "kiosk",
                table: "devices");

            migrationBuilder.AddColumn<int>(
                name: "slot_number",
                schema: "kiosk",
                table: "devices",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql("""
                WITH numbered_devices AS (
                    SELECT id, ROW_NUMBER() OVER (PARTITION BY tenant_id, kiosk_id, type ORDER BY sort_order, name, id) AS slot_number
                    FROM kiosk.devices
                )
                UPDATE kiosk.devices AS device
                SET slot_number = numbered_devices.slot_number
                FROM numbered_devices
                WHERE device.id = numbered_devices.id;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "slot_number",
                schema: "kiosk",
                table: "devices",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_kiosk_devices_tenant_kiosk_type_slot",
                schema: "kiosk",
                table: "devices",
                columns: new[] { "tenant_id", "kiosk_id", "type", "slot_number" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_kiosk_devices_tenant_kiosk_type_slot",
                schema: "kiosk",
                table: "devices");

            migrationBuilder.DropColumn(
                name: "slot_number",
                schema: "kiosk",
                table: "devices");

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
    }
}
