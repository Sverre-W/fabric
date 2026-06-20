using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabric.Server.Sagas.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVisitorPreOnboardingSagaConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "visitor_pre_onboarding_saga_configs",
                schema: "sagas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    use_custom_invite_notification = table.Column<bool>(type: "boolean", nullable: false),
                    custom_invite_notification = table.Column<string>(type: "text", nullable: true),
                    qr_generation_mode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    send_confirm_notification_to_organizer = table.Column<bool>(type: "boolean", nullable: false),
                    use_custom_confirm_notification = table.Column<bool>(type: "boolean", nullable: false),
                    custom_confirm_notification = table.Column<string>(type: "text", nullable: true),
                    send_cancellation_notification = table.Column<bool>(type: "boolean", nullable: false),
                    use_custom_cancellation_notification = table.Column<bool>(type: "boolean", nullable: false),
                    custom_cancellation_notification = table.Column<string>(type: "text", nullable: true),
                    send_reschedule_notification = table.Column<bool>(type: "boolean", nullable: false),
                    use_custom_reschedule_notification = table.Column<bool>(type: "boolean", nullable: false),
                    custom_reschedule_notification = table.Column<string>(type: "text", nullable: true),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_visitor_pre_onboarding_saga_configs", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_visitor_pre_onboarding_saga_configs_tenant_id",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs",
                column: "tenant_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "visitor_pre_onboarding_saga_configs",
                schema: "sagas");
        }
    }
}
