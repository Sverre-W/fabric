using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabric.Server.Sagas.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomNotificationSubjectBody : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "custom_reschedule_notification",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs",
                newName: "custom_reschedule_notification_body");

            migrationBuilder.RenameColumn(
                name: "custom_invite_notification",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs",
                newName: "custom_invite_notification_body");

            migrationBuilder.RenameColumn(
                name: "custom_confirm_notification",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs",
                newName: "custom_confirm_notification_body");

            migrationBuilder.RenameColumn(
                name: "custom_cancellation_notification",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs",
                newName: "custom_cancellation_notification_body");

            migrationBuilder.AddColumn<string>(
                name: "custom_cancellation_notification_subject",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "custom_confirm_notification_subject",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "custom_invite_notification_subject",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "custom_reschedule_notification_subject",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs",
                type: "text",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE sagas.visitor_pre_onboarding_saga_configs
                SET custom_invite_notification_subject = 'You''re invited to a visit'
                WHERE custom_invite_notification_body IS NOT NULL
                  AND custom_invite_notification_subject IS NULL;
                """);

            migrationBuilder.Sql("""
                UPDATE sagas.visitor_pre_onboarding_saga_configs
                SET custom_confirm_notification_subject = 'Visitor confirmed participation'
                WHERE custom_confirm_notification_body IS NOT NULL
                  AND custom_confirm_notification_subject IS NULL;
                """);

            migrationBuilder.Sql("""
                UPDATE sagas.visitor_pre_onboarding_saga_configs
                SET custom_cancellation_notification_subject = 'Your visit has been cancelled'
                WHERE custom_cancellation_notification_body IS NOT NULL
                  AND custom_cancellation_notification_subject IS NULL;
                """);

            migrationBuilder.Sql("""
                UPDATE sagas.visitor_pre_onboarding_saga_configs
                SET custom_reschedule_notification_subject = 'Your visit has been rescheduled'
                WHERE custom_reschedule_notification_body IS NOT NULL
                  AND custom_reschedule_notification_subject IS NULL;
                """);

            migrationBuilder.AddCheckConstraint(
                name: "ck_vpo_config_cancellation_notification_all_or_null",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs",
                sql: "(custom_cancellation_notification_subject IS NULL) = (custom_cancellation_notification_body IS NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_vpo_config_confirm_notification_all_or_null",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs",
                sql: "(custom_confirm_notification_subject IS NULL) = (custom_confirm_notification_body IS NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_vpo_config_invite_notification_all_or_null",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs",
                sql: "(custom_invite_notification_subject IS NULL) = (custom_invite_notification_body IS NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_vpo_config_reschedule_notification_all_or_null",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs",
                sql: "(custom_reschedule_notification_subject IS NULL) = (custom_reschedule_notification_body IS NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_vpo_config_cancellation_notification_all_or_null",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs");

            migrationBuilder.DropCheckConstraint(
                name: "ck_vpo_config_confirm_notification_all_or_null",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs");

            migrationBuilder.DropCheckConstraint(
                name: "ck_vpo_config_invite_notification_all_or_null",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs");

            migrationBuilder.DropCheckConstraint(
                name: "ck_vpo_config_reschedule_notification_all_or_null",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs");

            migrationBuilder.DropColumn(
                name: "custom_cancellation_notification_subject",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs");

            migrationBuilder.DropColumn(
                name: "custom_confirm_notification_subject",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs");

            migrationBuilder.DropColumn(
                name: "custom_invite_notification_subject",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs");

            migrationBuilder.DropColumn(
                name: "custom_reschedule_notification_subject",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs");

            migrationBuilder.RenameColumn(
                name: "custom_reschedule_notification_body",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs",
                newName: "custom_reschedule_notification");

            migrationBuilder.RenameColumn(
                name: "custom_invite_notification_body",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs",
                newName: "custom_invite_notification");

            migrationBuilder.RenameColumn(
                name: "custom_confirm_notification_body",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs",
                newName: "custom_confirm_notification");

            migrationBuilder.RenameColumn(
                name: "custom_cancellation_notification_body",
                schema: "sagas",
                table: "visitor_pre_onboarding_saga_configs",
                newName: "custom_cancellation_notification");
        }
    }
}
