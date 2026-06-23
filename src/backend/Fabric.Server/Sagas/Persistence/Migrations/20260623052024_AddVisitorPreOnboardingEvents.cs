using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabric.Server.Sagas.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVisitorPreOnboardingEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "arrival_notification_sent_at",
                schema: "sagas",
                table: "visitor_pre_onboarding_sagas",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "custom_arrival_notification_body",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "custom_arrival_notification_subject",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "send_arrival_notification_to_organizer",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "use_custom_arrival_notification",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "visitor_pre_onboarding_saga_events",
                schema: "sagas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    saga_id = table.Column<Guid>(type: "uuid", nullable: true),
                    visit_id = table.Column<Guid>(type: "uuid", nullable: true),
                    invitation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    arrival_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    next_retry_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    failure_reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_visitor_pre_onboarding_saga_events", x => x.id);
                });

            migrationBuilder.AddCheckConstraint(
                name: "ck_vpo_config_arrival_notification_all_or_null",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs",
                sql: "(custom_arrival_notification_subject IS NULL) = (custom_arrival_notification_body IS NULL)");

            migrationBuilder.CreateIndex(
                name: "ix_visitor_pre_onboarding_saga_events_tenant_id",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_events",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_vpo_saga_events_created_at",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_events",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_vpo_saga_events_next_retry_at",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_events",
                column: "next_retry_at");

            migrationBuilder.CreateIndex(
                name: "ix_vpo_saga_events_processed_at",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_events",
                column: "processed_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "visitor_pre_onboarding_saga_events",
                schema: "sagas");

            migrationBuilder.DropCheckConstraint(
                name: "ck_vpo_config_arrival_notification_all_or_null",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs");

            migrationBuilder.DropColumn(
                name: "arrival_notification_sent_at",
                schema: "sagas",
                table: "visitor_pre_onboarding_sagas");

            migrationBuilder.DropColumn(
                name: "custom_arrival_notification_body",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs");

            migrationBuilder.DropColumn(
                name: "custom_arrival_notification_subject",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs");

            migrationBuilder.DropColumn(
                name: "send_arrival_notification_to_organizer",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs");

            migrationBuilder.DropColumn(
                name: "use_custom_arrival_notification",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs");
        }
    }
}
