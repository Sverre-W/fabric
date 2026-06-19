using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabric.Server.Reception.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReceptionInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "reception");

            migrationBuilder.CreateTable(
                name: "expected_arrivals",
                schema: "reception",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    expected_arrival_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    arrival_code = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    onboarded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    offboarded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    checked_in = table.Column<bool>(type: "boolean", nullable: false),
                    location_id = table.Column<Guid>(type: "uuid", nullable: true),
                    confirmed = table.Column<bool>(type: "boolean", nullable: true),
                    visitor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    invitation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    contractor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    job_assignment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    first_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    last_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    company = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_expected_arrivals", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "arrival_entries",
                schema: "reception",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expected_arrival_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_arrival_entries", x => x.id);
                    table.ForeignKey(
                        name: "fk_arrival_entries_expected_arrivals_expected_arrival_id",
                        column: x => x.expected_arrival_id,
                        principalSchema: "reception",
                        principalTable: "expected_arrivals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "check_in_documents",
                schema: "reception",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    document_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    content = table.Column<byte[]>(type: "bytea", nullable: false),
                    expected_arrival_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_check_in_documents", x => x.id);
                    table.ForeignKey(
                        name: "fk_check_in_documents_expected_arrivals_expected_arrival_id",
                        column: x => x.expected_arrival_id,
                        principalSchema: "reception",
                        principalTable: "expected_arrivals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_arrival_entries_expected_arrival_id",
                schema: "reception",
                table: "arrival_entries",
                column: "expected_arrival_id");

            migrationBuilder.CreateIndex(
                name: "ix_check_in_documents_expected_arrival_id",
                schema: "reception",
                table: "check_in_documents",
                column: "expected_arrival_id");

            migrationBuilder.CreateIndex(
                name: "ix_expected_arrivals_arrival_code",
                schema: "reception",
                table: "expected_arrivals",
                column: "arrival_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_expected_arrivals_contractor_id",
                schema: "reception",
                table: "expected_arrivals",
                column: "contractor_id");

            migrationBuilder.CreateIndex(
                name: "ix_expected_arrivals_location_id",
                schema: "reception",
                table: "expected_arrivals",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "ix_expected_arrivals_visitor_id",
                schema: "reception",
                table: "expected_arrivals",
                column: "visitor_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "arrival_entries",
                schema: "reception");

            migrationBuilder.DropTable(
                name: "check_in_documents",
                schema: "reception");

            migrationBuilder.DropTable(
                name: "expected_arrivals",
                schema: "reception");
        }
    }
}
