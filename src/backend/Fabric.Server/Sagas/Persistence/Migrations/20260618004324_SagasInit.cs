using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabric.Server.Sagas.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SagasInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "sagas");

            migrationBuilder.CreateTable(
                name: "visitor_pre_onboarding_sagas",
                schema: "sagas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    visit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    invitation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    arrival_id = table.Column<Guid>(type: "uuid", nullable: true),
                    qr_code = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    next_retry_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    state = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_visitor_pre_onboarding_sagas", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_visitor_pre_onboarding_sagas_next_retry_at",
                schema: "sagas",
                table: "visitor_pre_onboarding_sagas",
                column: "next_retry_at");

            migrationBuilder.CreateIndex(
                name: "ix_visitor_pre_onboarding_sagas_state",
                schema: "sagas",
                table: "visitor_pre_onboarding_sagas",
                column: "state");

            migrationBuilder.CreateIndex(
                name: "ix_visitor_pre_onboarding_sagas_visit_invitation",
                schema: "sagas",
                table: "visitor_pre_onboarding_sagas",
                columns: new[] { "visit_id", "invitation_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "visitor_pre_onboarding_sagas",
                schema: "sagas");
        }
    }
}
