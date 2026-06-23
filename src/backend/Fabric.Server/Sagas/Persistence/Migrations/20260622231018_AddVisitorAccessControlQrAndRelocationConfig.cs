using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabric.Server.Sagas.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVisitorAccessControlQrAndRelocationConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "access_policy_id",
                schema: "sagas",
                table: "visitor_pre_onboarding_sagas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "access_level_type_id",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "custom_relocation_notification_body",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "custom_relocation_notification_subject",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "send_relocation_notification",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "system_id",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "use_custom_relocation_notification",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddCheckConstraint(
                name: "ck_vpo_config_access_control_qr_ids",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs",
                sql: "(qr_generation_mode <> 'AccessControlQr') OR (system_id IS NOT NULL AND access_level_type_id IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_vpo_config_relocation_notification_all_or_null",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs",
                sql: "(custom_relocation_notification_subject IS NULL) = (custom_relocation_notification_body IS NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_vpo_config_access_control_qr_ids",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs");

            migrationBuilder.DropCheckConstraint(
                name: "ck_vpo_config_relocation_notification_all_or_null",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs");

            migrationBuilder.DropColumn(
                name: "access_policy_id",
                schema: "sagas",
                table: "visitor_pre_onboarding_sagas");

            migrationBuilder.DropColumn(
                name: "access_level_type_id",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs");

            migrationBuilder.DropColumn(
                name: "custom_relocation_notification_body",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs");

            migrationBuilder.DropColumn(
                name: "custom_relocation_notification_subject",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs");

            migrationBuilder.DropColumn(
                name: "send_relocation_notification",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs");

            migrationBuilder.DropColumn(
                name: "system_id",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs");

            migrationBuilder.DropColumn(
                name: "use_custom_relocation_notification",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs");
        }
    }
}
