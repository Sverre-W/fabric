using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabric.Server.Sagas.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UseVisitorBadgeTypeConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_vpo_config_access_control_qr_ids",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs");

            migrationBuilder.RenameColumn(
                name: "access_level_type_id",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs",
                newName: "badge_type_id");

            migrationBuilder.AddCheckConstraint(
                name: "ck_vpo_config_access_control_qr_ids",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs",
                sql: "(qr_generation_mode <> 'AccessControlQr') OR (system_id IS NOT NULL AND badge_type_id IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_vpo_config_access_control_qr_ids",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs");

            migrationBuilder.RenameColumn(
                name: "badge_type_id",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs",
                newName: "access_level_type_id");

            migrationBuilder.AddCheckConstraint(
                name: "ck_vpo_config_access_control_qr_ids",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs",
                sql: "(qr_generation_mode <> 'AccessControlQr') OR (system_id IS NOT NULL AND access_level_type_id IS NOT NULL)");
        }
    }
}
