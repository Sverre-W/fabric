using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabric.Server.AccessPolicies.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AccessPoliciesUnipassReconciliation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "issued_provider_resources",
                schema: "access_policies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    policy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_id = table.Column<Guid>(type: "uuid", nullable: false),
                    system_id = table.Column<Guid>(type: "uuid", nullable: false),
                    resource_kind = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    badge_type_id = table.Column<Guid>(type: "uuid", nullable: true),
                    badge_number = table.Column<int>(type: "integer", nullable: true),
                    access_level_type_id = table.Column<Guid>(type: "uuid", nullable: true),
                    external_person_id = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    external_resource_id = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_issued_provider_resources", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "used_badge_numbers",
                schema: "access_policies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    system_id = table.Column<Guid>(type: "uuid", nullable: false),
                    badge_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_id = table.Column<Guid>(type: "uuid", nullable: false),
                    badge_number = table.Column<int>(type: "integer", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_used_badge_numbers", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_issued_provider_resources_policy_id",
                schema: "access_policies",
                table: "issued_provider_resources",
                column: "policy_id");

            migrationBuilder.CreateIndex(
                name: "ix_issued_provider_resources_subject_id_system_id",
                schema: "access_policies",
                table: "issued_provider_resources",
                columns: new[] { "subject_id", "system_id" });

            migrationBuilder.CreateIndex(
                name: "ix_issued_provider_resources_tenant_id",
                schema: "access_policies",
                table: "issued_provider_resources",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_used_badge_numbers_tenant_id",
                schema: "access_policies",
                table: "used_badge_numbers",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_used_badge_numbers_tenant_id_system_id_badge_type_id_badge_number",
                schema: "access_policies",
                table: "used_badge_numbers",
                columns: new[] { "tenant_id", "system_id", "badge_type_id", "badge_number" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "issued_provider_resources",
                schema: "access_policies");

            migrationBuilder.DropTable(
                name: "used_badge_numbers",
                schema: "access_policies");
        }
    }
}
