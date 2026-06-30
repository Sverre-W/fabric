using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabric.Server.Hardware.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHardwareRegistry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "hardware");

            migrationBuilder.CreateTable(
                name: "agents",
                schema: "hardware",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    ApiKeyHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ApiKeySalt = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LastSeenAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastInventoryAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "devices",
                schema: "hardware",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DeviceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Kind = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Driver = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Capabilities = table.Column<string[]>(type: "text[]", nullable: false),
                    State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    DiagnosticsJson = table.Column<string>(type: "jsonb", nullable: false),
                    LastSeenAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_devices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "event_inbox",
                schema: "hardware",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DeviceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event_inbox", x => x.EventId);
                });

            migrationBuilder.CreateIndex(
                name: "ix_agents_tenant_id",
                schema: "hardware",
                table: "agents",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_devices_AgentId_DeviceId",
                schema: "hardware",
                table: "devices",
                columns: new[] { "AgentId", "DeviceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_devices_tenant_id",
                schema: "hardware",
                table: "devices",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_event_inbox_tenant_id",
                schema: "hardware",
                table: "event_inbox",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agents",
                schema: "hardware");

            migrationBuilder.DropTable(
                name: "devices",
                schema: "hardware");

            migrationBuilder.DropTable(
                name: "event_inbox",
                schema: "hardware");
        }
    }
}
