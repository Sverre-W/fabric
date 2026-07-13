using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabric.Server.Kiosk.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddKioskTerminalDisplay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "terminal_message",
                schema: "kiosk",
                table: "sessions",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "terminal_title",
                schema: "kiosk",
                table: "sessions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "show_detailed_errors",
                schema: "kiosk",
                table: "kiosks",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "terminal_message",
                schema: "kiosk",
                table: "sessions");

            migrationBuilder.DropColumn(
                name: "terminal_title",
                schema: "kiosk",
                table: "sessions");

            migrationBuilder.DropColumn(
                name: "show_detailed_errors",
                schema: "kiosk",
                table: "kiosks");
        }
    }
}
