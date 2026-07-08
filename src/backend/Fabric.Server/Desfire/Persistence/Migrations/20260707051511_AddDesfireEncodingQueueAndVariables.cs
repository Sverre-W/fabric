using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabric.Server.Desfire.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDesfireEncodingQueueAndVariables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AttemptCount",
                schema: "desfire",
                table: "encoding_runs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ClaimExpiresAt",
                schema: "desfire",
                table: "encoding_runs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ClaimedAt",
                schema: "desfire",
                table: "encoding_runs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClaimedBy",
                schema: "desfire",
                table: "encoding_runs",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                schema: "desfire",
                table: "encoding_runs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RequestedAgentId",
                schema: "desfire",
                table: "encoding_runs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestedDeviceId",
                schema: "desfire",
                table: "encoding_runs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VariableConfigJson",
                schema: "desfire",
                table: "encoding_runs",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.CreateTable(
                name: "variable_sequences",
                schema: "desfire",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NextValue = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_variable_sequences", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_encoding_runs_RequestedAgentId_RequestedDeviceId",
                schema: "desfire",
                table: "encoding_runs",
                columns: new[] { "RequestedAgentId", "RequestedDeviceId" });

            migrationBuilder.CreateIndex(
                name: "IX_encoding_runs_Status_Priority_RequestedAt",
                schema: "desfire",
                table: "encoding_runs",
                columns: new[] { "Status", "Priority", "RequestedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_variable_sequences_Name",
                schema: "desfire",
                table: "variable_sequences",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_variable_sequences_tenant_id",
                schema: "desfire",
                table: "variable_sequences",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "variable_sequences",
                schema: "desfire");

            migrationBuilder.DropIndex(
                name: "IX_encoding_runs_RequestedAgentId_RequestedDeviceId",
                schema: "desfire",
                table: "encoding_runs");

            migrationBuilder.DropIndex(
                name: "IX_encoding_runs_Status_Priority_RequestedAt",
                schema: "desfire",
                table: "encoding_runs");

            migrationBuilder.DropColumn(
                name: "AttemptCount",
                schema: "desfire",
                table: "encoding_runs");

            migrationBuilder.DropColumn(
                name: "ClaimExpiresAt",
                schema: "desfire",
                table: "encoding_runs");

            migrationBuilder.DropColumn(
                name: "ClaimedAt",
                schema: "desfire",
                table: "encoding_runs");

            migrationBuilder.DropColumn(
                name: "ClaimedBy",
                schema: "desfire",
                table: "encoding_runs");

            migrationBuilder.DropColumn(
                name: "Priority",
                schema: "desfire",
                table: "encoding_runs");

            migrationBuilder.DropColumn(
                name: "RequestedAgentId",
                schema: "desfire",
                table: "encoding_runs");

            migrationBuilder.DropColumn(
                name: "RequestedDeviceId",
                schema: "desfire",
                table: "encoding_runs");

            migrationBuilder.DropColumn(
                name: "VariableConfigJson",
                schema: "desfire",
                table: "encoding_runs");
        }
    }
}
