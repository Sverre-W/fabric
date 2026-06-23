using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabric.Server.AccessPolicies.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAccessPolicySatisfiedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "satisfied_by_access_level_type_id",
                schema: "access_policies",
                table: "access_policies",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "satisfied_by_badge_number",
                schema: "access_policies",
                table: "access_policies",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "satisfied_by_badge_type_id",
                schema: "access_policies",
                table: "access_policies",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "satisfied_by_kind",
                schema: "access_policies",
                table: "access_policies",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "satisfied_by_subject_id",
                schema: "access_policies",
                table: "access_policies",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "satisfied_by_system_id",
                schema: "access_policies",
                table: "access_policies",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "satisfied_by_access_level_type_id",
                schema: "access_policies",
                table: "access_policies");

            migrationBuilder.DropColumn(
                name: "satisfied_by_badge_number",
                schema: "access_policies",
                table: "access_policies");

            migrationBuilder.DropColumn(
                name: "satisfied_by_badge_type_id",
                schema: "access_policies",
                table: "access_policies");

            migrationBuilder.DropColumn(
                name: "satisfied_by_kind",
                schema: "access_policies",
                table: "access_policies");

            migrationBuilder.DropColumn(
                name: "satisfied_by_subject_id",
                schema: "access_policies",
                table: "access_policies");

            migrationBuilder.DropColumn(
                name: "satisfied_by_system_id",
                schema: "access_policies",
                table: "access_policies");
        }
    }
}
