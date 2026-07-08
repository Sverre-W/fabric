using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabric.Server.Sagas.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddKioskSaga : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "kiosk_saga_events",
                schema: "sagas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    saga_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    instruction_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    instruction_kind = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    result_json = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    next_retry_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    failure_reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_kiosk_saga_events", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "kiosk_sagas",
                schema: "sagas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    workflow_instance_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    current_instruction_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    current_instruction_kind = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    state = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_kiosk_sagas", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_kiosk_saga_events_created_at",
                schema: "sagas",
                table: "kiosk_saga_events",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_kiosk_saga_events_next_retry_at",
                schema: "sagas",
                table: "kiosk_saga_events",
                column: "next_retry_at");

            migrationBuilder.CreateIndex(
                name: "ix_kiosk_saga_events_processed_at",
                schema: "sagas",
                table: "kiosk_saga_events",
                column: "processed_at");

            migrationBuilder.CreateIndex(
                name: "ix_kiosk_saga_events_tenant_id",
                schema: "sagas",
                table: "kiosk_saga_events",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_kiosk_sagas_session_id",
                schema: "sagas",
                table: "kiosk_sagas",
                column: "session_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_kiosk_sagas_tenant_id",
                schema: "sagas",
                table: "kiosk_sagas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_kiosk_sagas_workflow_instance_id",
                schema: "sagas",
                table: "kiosk_sagas",
                column: "workflow_instance_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "kiosk_saga_events",
                schema: "sagas");

            migrationBuilder.DropTable(
                name: "kiosk_sagas",
                schema: "sagas");
        }
    }
}
