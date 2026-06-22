using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabric.Server.Reception.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReceptionAccessRuleAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "expected_offboard_time",
                schema: "reception",
                table: "expected_arrivals",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE reception.expected_arrivals
                SET expected_offboard_time = expected_arrival_time + INTERVAL '1 hour'
                WHERE expected_offboard_time IS NULL
                """);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "expected_offboard_time",
                schema: "reception",
                table: "expected_arrivals",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "access_rule_assignments",
                schema: "reception",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    location_id = table.Column<Guid>(type: "uuid", nullable: false),
                    system_id = table.Column<Guid>(type: "uuid", nullable: false),
                    access_level_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    grace_period_minutes = table.Column<int>(type: "integer", nullable: false),
                    trigger = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_access_rule_assignments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "assigned_access_policies",
                schema: "reception",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    arrival_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rule_assignment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    access_policy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    system_id = table.Column<Guid>(type: "uuid", nullable: false),
                    access_level_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_assigned_access_policies", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_access_rule_assignments_access_level_type_id",
                schema: "reception",
                table: "access_rule_assignments",
                column: "access_level_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_access_rule_assignments_location_id",
                schema: "reception",
                table: "access_rule_assignments",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "ix_access_rule_assignments_system_id",
                schema: "reception",
                table: "access_rule_assignments",
                column: "system_id");

            migrationBuilder.CreateIndex(
                name: "ix_access_rule_assignments_tenant_id",
                schema: "reception",
                table: "access_rule_assignments",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_access_rule_assignments_trigger",
                schema: "reception",
                table: "access_rule_assignments",
                column: "trigger");

            migrationBuilder.CreateIndex(
                name: "ix_assigned_access_policies_access_policy_id",
                schema: "reception",
                table: "assigned_access_policies",
                column: "access_policy_id");

            migrationBuilder.CreateIndex(
                name: "ix_assigned_access_policies_arrival_id",
                schema: "reception",
                table: "assigned_access_policies",
                column: "arrival_id");

            migrationBuilder.CreateIndex(
                name: "ix_assigned_access_policies_arrival_id_rule_assignment_id",
                schema: "reception",
                table: "assigned_access_policies",
                columns: new[] { "arrival_id", "rule_assignment_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_assigned_access_policies_rule_assignment_id",
                schema: "reception",
                table: "assigned_access_policies",
                column: "rule_assignment_id");

            migrationBuilder.CreateIndex(
                name: "ix_assigned_access_policies_tenant_id",
                schema: "reception",
                table: "assigned_access_policies",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "access_rule_assignments",
                schema: "reception");

            migrationBuilder.DropTable(
                name: "assigned_access_policies",
                schema: "reception");

            migrationBuilder.DropColumn(
                name: "expected_offboard_time",
                schema: "reception",
                table: "expected_arrivals");
        }
    }
}
