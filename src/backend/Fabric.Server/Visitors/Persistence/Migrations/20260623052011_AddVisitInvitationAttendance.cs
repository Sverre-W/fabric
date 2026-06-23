using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabric.Server.Visitors.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVisitInvitationAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "arrived_at",
                schema: "visitors",
                table: "visit_invitations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "no_show_at",
                schema: "visitors",
                table: "visit_invitations",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "arrived_at",
                schema: "visitors",
                table: "visit_invitations");

            migrationBuilder.DropColumn(
                name: "no_show_at",
                schema: "visitors",
                table: "visit_invitations");
        }
    }
}
