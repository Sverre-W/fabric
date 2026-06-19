using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabric.Server.Visitors.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class VisitorsInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "visitors");

            migrationBuilder.CreateTable(
                name: "organizers",
                schema: "visitors",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    last_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organizers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "visitors",
                schema: "visitors",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    last_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    company = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_visitors", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "visits",
                schema: "visitors",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    summary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    organizer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    stop = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    location_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_visits", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "visit_invitations",
                schema: "visitors",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    last_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    company = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    confirmation_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    visitor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    rejected_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    confirmed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    transport = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    license_plate = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    visit_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_visit_invitations", x => x.id);
                    table.ForeignKey(
                        name: "fk_visit_invitations_visits_visit_id",
                        column: x => x.visit_id,
                        principalSchema: "visitors",
                        principalTable: "visits",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_organizers_email",
                schema: "visitors",
                table: "organizers",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_visit_invitations_visit_id_email",
                schema: "visitors",
                table: "visit_invitations",
                columns: new[] { "visit_id", "email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_visitors_email",
                schema: "visitors",
                table: "visitors",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "organizers",
                schema: "visitors");

            migrationBuilder.DropTable(
                name: "visit_invitations",
                schema: "visitors");

            migrationBuilder.DropTable(
                name: "visitors",
                schema: "visitors");

            migrationBuilder.DropTable(
                name: "visits",
                schema: "visitors");
        }
    }
}
