using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabric.Server.Desfire.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDesfireEncoders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EncoderId",
                schema: "desfire",
                table: "encoding_runs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EncoderId",
                schema: "desfire",
                table: "encoding_batches",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "encoders",
                schema: "desfire",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AgentId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DeviceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SupportsEncoding = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsPrinting = table.Column<bool>(type: "boolean", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_encoders", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_encoding_runs_EncoderId",
                schema: "desfire",
                table: "encoding_runs",
                column: "EncoderId");

            migrationBuilder.CreateIndex(
                name: "IX_encoding_batches_EncoderId",
                schema: "desfire",
                table: "encoding_batches",
                column: "EncoderId");

            migrationBuilder.CreateIndex(
                name: "ix_encoders_tenant_id",
                schema: "desfire",
                table: "encoders",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_encoders_tenant_id_agent_id_device_id",
                schema: "desfire",
                table: "encoders",
                columns: new[] { "tenant_id", "AgentId", "DeviceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_encoders_tenant_id_name",
                schema: "desfire",
                table: "encoders",
                columns: new[] { "tenant_id", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "encoders",
                schema: "desfire");

            migrationBuilder.DropIndex(
                name: "IX_encoding_runs_EncoderId",
                schema: "desfire",
                table: "encoding_runs");

            migrationBuilder.DropIndex(
                name: "IX_encoding_batches_EncoderId",
                schema: "desfire",
                table: "encoding_batches");

            migrationBuilder.DropColumn(
                name: "EncoderId",
                schema: "desfire",
                table: "encoding_runs");

            migrationBuilder.DropColumn(
                name: "EncoderId",
                schema: "desfire",
                table: "encoding_batches");
        }
    }
}
