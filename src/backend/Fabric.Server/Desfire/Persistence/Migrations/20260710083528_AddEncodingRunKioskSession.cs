using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabric.Server.Desfire.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEncodingRunKioskSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "KioskSessionId",
                schema: "desfire",
                table: "encoding_runs",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_encoding_runs_KioskSessionId",
                schema: "desfire",
                table: "encoding_runs",
                column: "KioskSessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_encoding_runs_KioskSessionId",
                schema: "desfire",
                table: "encoding_runs");

            migrationBuilder.DropColumn(
                name: "KioskSessionId",
                schema: "desfire",
                table: "encoding_runs");
        }
    }
}
