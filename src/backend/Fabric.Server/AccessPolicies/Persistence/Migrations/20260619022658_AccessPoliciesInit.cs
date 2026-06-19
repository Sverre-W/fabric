using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabric.Server.AccessPolicies.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AccessPoliciesInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "access_policies");

            migrationBuilder.CreateTable(
                name: "access_control_systems",
                schema: "access_policies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    provider = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    lenel_endpoint = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    lenel_ssl_validation = table.Column<bool>(type: "boolean", nullable: true),
                    lenel_api_key = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    unipass_endpoint = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    unipass_ssl_validation = table.Column<bool>(type: "boolean", nullable: true),
                    unipass_username = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    unipass_password = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_access_control_systems", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "access_policies",
                schema: "access_policies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    system_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_first_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    subject_last_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    subject_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    effective_from = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    effective_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    reconciliation_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    reconciliation_failure_reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_access_policies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "identity_mappings",
                schema: "access_policies",
                columns: table => new
                {
                    subject_id = table.Column<Guid>(type: "uuid", nullable: false),
                    system_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_first_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    subject_last_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    subject_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    external_id = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_mappings", x => new { x.subject_id, x.system_id });
                });

            migrationBuilder.CreateTable(
                name: "access_level_types",
                schema: "access_policies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    system_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    provider = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    access_level_id = table.Column<Guid>(type: "uuid", nullable: true),
                    site_id = table.Column<int>(type: "integer", nullable: true),
                    access_rule_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_access_level_types", x => x.id);
                    table.ForeignKey(
                        name: "fk_access_level_types_access_control_systems_system_id",
                        column: x => x.system_id,
                        principalSchema: "access_policies",
                        principalTable: "access_control_systems",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "badge_types",
                schema: "access_policies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    system_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    provider = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    badge_type_id = table.Column<Guid>(type: "uuid", nullable: true),
                    range_start = table.Column<int>(type: "integer", nullable: true),
                    range_stop = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_badge_types", x => x.id);
                    table.ForeignKey(
                        name: "fk_badge_types_access_control_systems_system_id",
                        column: x => x.system_id,
                        principalSchema: "access_policies",
                        principalTable: "access_control_systems",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lenel_access_level_type_badge_types",
                schema: "access_policies",
                columns: table => new
                {
                    access_level_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    badge_type_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lenel_access_level_type_badge_types", x => new { x.access_level_type_id, x.badge_type_id });
                    table.ForeignKey(
                        name: "fk_lenel_access_level_type_badge_types_access_level_types_access_level_type_id",
                        column: x => x.access_level_type_id,
                        principalSchema: "access_policies",
                        principalTable: "access_level_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_lenel_access_level_type_badge_types_badge_types_badge_type_id",
                        column: x => x.badge_type_id,
                        principalSchema: "access_policies",
                        principalTable: "badge_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "policy_requirements",
                schema: "access_policies",
                columns: table => new
                {
                    access_policy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    requirement_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    access_level_type_id = table.Column<Guid>(type: "uuid", nullable: true),
                    badge_type_id = table.Column<Guid>(type: "uuid", nullable: true),
                    badge_number = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_policy_requirements", x => x.access_policy_id);
                    table.ForeignKey(
                        name: "fk_policy_requirements_access_level_types_access_level_type_id",
                        column: x => x.access_level_type_id,
                        principalSchema: "access_policies",
                        principalTable: "access_level_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_policy_requirements_access_policies_access_policy_id",
                        column: x => x.access_policy_id,
                        principalSchema: "access_policies",
                        principalTable: "access_policies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_policy_requirements_badge_types_badge_type_id",
                        column: x => x.badge_type_id,
                        principalSchema: "access_policies",
                        principalTable: "badge_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_access_control_systems_tenant_id",
                schema: "access_policies",
                table: "access_control_systems",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_access_control_systems_tenant_id_name",
                schema: "access_policies",
                table: "access_control_systems",
                columns: new[] { "tenant_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_access_level_types_system_id",
                schema: "access_policies",
                table: "access_level_types",
                column: "system_id");

            migrationBuilder.CreateIndex(
                name: "ix_access_level_types_tenant_id",
                schema: "access_policies",
                table: "access_level_types",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_access_level_types_tenant_id_system_id_access_level_id",
                schema: "access_policies",
                table: "access_level_types",
                columns: new[] { "tenant_id", "system_id", "access_level_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_access_level_types_tenant_id_system_id_name",
                schema: "access_policies",
                table: "access_level_types",
                columns: new[] { "tenant_id", "system_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_access_level_types_tenant_id_system_id_site_id_access_rule_id",
                schema: "access_policies",
                table: "access_level_types",
                columns: new[] { "tenant_id", "system_id", "site_id", "access_rule_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_access_policies_effective_until",
                schema: "access_policies",
                table: "access_policies",
                column: "effective_until");

            migrationBuilder.CreateIndex(
                name: "ix_access_policies_reconciliation_status",
                schema: "access_policies",
                table: "access_policies",
                column: "reconciliation_status");

            migrationBuilder.CreateIndex(
                name: "ix_access_policies_system_id",
                schema: "access_policies",
                table: "access_policies",
                column: "system_id");

            migrationBuilder.CreateIndex(
                name: "ix_access_policies_tenant_id",
                schema: "access_policies",
                table: "access_policies",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_badge_types_system_id",
                schema: "access_policies",
                table: "badge_types",
                column: "system_id");

            migrationBuilder.CreateIndex(
                name: "ix_badge_types_tenant_id",
                schema: "access_policies",
                table: "badge_types",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_badge_types_tenant_id_system_id_badge_type_id",
                schema: "access_policies",
                table: "badge_types",
                columns: new[] { "tenant_id", "system_id", "badge_type_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_badge_types_tenant_id_system_id_name",
                schema: "access_policies",
                table: "badge_types",
                columns: new[] { "tenant_id", "system_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_identity_mappings_external_id",
                schema: "access_policies",
                table: "identity_mappings",
                column: "external_id");

            migrationBuilder.CreateIndex(
                name: "ix_identity_mappings_tenant_id",
                schema: "access_policies",
                table: "identity_mappings",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_lenel_access_level_type_badge_types_badge_type_id",
                schema: "access_policies",
                table: "lenel_access_level_type_badge_types",
                column: "badge_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_policy_requirements_access_level_type_id",
                schema: "access_policies",
                table: "policy_requirements",
                column: "access_level_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_policy_requirements_badge_type_id",
                schema: "access_policies",
                table: "policy_requirements",
                column: "badge_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_policy_requirements_tenant_id",
                schema: "access_policies",
                table: "policy_requirements",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "identity_mappings",
                schema: "access_policies");

            migrationBuilder.DropTable(
                name: "lenel_access_level_type_badge_types",
                schema: "access_policies");

            migrationBuilder.DropTable(
                name: "policy_requirements",
                schema: "access_policies");

            migrationBuilder.DropTable(
                name: "access_level_types",
                schema: "access_policies");

            migrationBuilder.DropTable(
                name: "access_policies",
                schema: "access_policies");

            migrationBuilder.DropTable(
                name: "badge_types",
                schema: "access_policies");

            migrationBuilder.DropTable(
                name: "access_control_systems",
                schema: "access_policies");
        }
    }
}
