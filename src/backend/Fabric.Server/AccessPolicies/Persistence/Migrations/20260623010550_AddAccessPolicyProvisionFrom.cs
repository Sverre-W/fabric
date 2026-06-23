using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabric.Server.AccessPolicies.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAccessPolicyProvisionFrom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "provision_from",
                schema: "access_policies",
                table: "access_policies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql("UPDATE access_policies.access_policies SET provision_from = effective_from;");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "provision_from",
                schema: "access_policies",
                table: "access_policies",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "provision_from",
                schema: "access_policies",
                table: "access_policies");
        }
    }
}
