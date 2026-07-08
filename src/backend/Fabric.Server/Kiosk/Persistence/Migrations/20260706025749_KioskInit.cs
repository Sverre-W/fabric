using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabric.Server.Kiosk.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class KioskInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "kiosk");

            migrationBuilder.CreateTable(
                name: "assets",
                schema: "kiosk",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    language_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    kind = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    file_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    content_type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    size = table.Column<long>(type: "bigint", nullable: false),
                    relative_path = table.Column<string>(type: "character varying(800)", maxLength: 800, nullable: false),
                    alt_text_key = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_kiosk_assets", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "device_assignments",
                schema: "kiosk",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kiosk_id = table.Column<Guid>(type: "uuid", nullable: false),
                    binding_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    agent_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    device_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    enabled = table.Column<bool>(type: "boolean", nullable: false),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_kiosk_device_assignments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "hardware_bindings",
                schema: "kiosk",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    binding_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    required_capability = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    required = table.Column<bool>(type: "boolean", nullable: false),
                    cleanup_on_session_end = table.Column<bool>(type: "boolean", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_kiosk_hardware_bindings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "kiosks",
                schema: "kiosk",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    mode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    api_key_hash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    api_key_salt = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    workflow_definition_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    last_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_kiosks", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "profile_languages",
                schema: "kiosk",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_kiosk_profile_languages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "profiles",
                schema: "kiosk",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    default_language_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_kiosk_profiles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sessions",
                schema: "kiosk",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kiosk_id = table.Column<Guid>(type: "uuid", nullable: false),
                    workflow_instance_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    language_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    current_instruction_json = table.Column<string>(type: "jsonb", nullable: true),
                    current_instruction_version = table.Column<int>(type: "integer", nullable: false),
                    current_instruction_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_interaction_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_kiosk_sessions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "theme_tokens",
                schema: "kiosk",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_kiosk_theme_tokens", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "translations",
                schema: "kiosk",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    key = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    value = table.Column<string>(type: "text", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_kiosk_translations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "welcome_settings",
                schema: "kiosk",
                columns: table => new
                {
                    profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title_key = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    subtitle_key = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    start_button_key = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    background_asset_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    logo_asset_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_kiosk_welcome_settings", x => x.profile_id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_assets_tenant_id",
                schema: "kiosk",
                table: "assets",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_kiosk_assets_tenant_profile_name_language",
                schema: "kiosk",
                table: "assets",
                columns: new[] { "tenant_id", "profile_id", "name", "language_code" });

            migrationBuilder.CreateIndex(
                name: "ix_device_assignments_tenant_id",
                schema: "kiosk",
                table: "device_assignments",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_kiosk_device_assignments_tenant_kiosk_binding_priority",
                schema: "kiosk",
                table: "device_assignments",
                columns: new[] { "tenant_id", "kiosk_id", "binding_key", "priority" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_hardware_bindings_tenant_id",
                schema: "kiosk",
                table: "hardware_bindings",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_kiosk_hardware_bindings_tenant_profile_key",
                schema: "kiosk",
                table: "hardware_bindings",
                columns: new[] { "tenant_id", "profile_id", "binding_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_kiosks_tenant_id",
                schema: "kiosk",
                table: "kiosks",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_kiosks_tenant_id_name",
                schema: "kiosk",
                table: "kiosks",
                columns: new[] { "tenant_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_kiosks_tenant_id_profile_id",
                schema: "kiosk",
                table: "kiosks",
                columns: new[] { "tenant_id", "profile_id" });

            migrationBuilder.CreateIndex(
                name: "ix_kiosk_profile_languages_tenant_id_profile_id_language_code",
                schema: "kiosk",
                table: "profile_languages",
                columns: new[] { "tenant_id", "profile_id", "language_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_profile_languages_tenant_id",
                schema: "kiosk",
                table: "profile_languages",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_kiosk_profiles_tenant_id_name",
                schema: "kiosk",
                table: "profiles",
                columns: new[] { "tenant_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_profiles_tenant_id",
                schema: "kiosk",
                table: "profiles",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_kiosk_sessions_tenant_kiosk_status",
                schema: "kiosk",
                table: "sessions",
                columns: new[] { "tenant_id", "kiosk_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_sessions_tenant_id",
                schema: "kiosk",
                table: "sessions",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_kiosk_theme_tokens_tenant_profile_key",
                schema: "kiosk",
                table: "theme_tokens",
                columns: new[] { "tenant_id", "profile_id", "key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_theme_tokens_tenant_id",
                schema: "kiosk",
                table: "theme_tokens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_kiosk_translations_tenant_profile_language_key",
                schema: "kiosk",
                table: "translations",
                columns: new[] { "tenant_id", "profile_id", "language_code", "key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_translations_tenant_id",
                schema: "kiosk",
                table: "translations",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_welcome_settings_tenant_id",
                schema: "kiosk",
                table: "welcome_settings",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assets",
                schema: "kiosk");

            migrationBuilder.DropTable(
                name: "device_assignments",
                schema: "kiosk");

            migrationBuilder.DropTable(
                name: "hardware_bindings",
                schema: "kiosk");

            migrationBuilder.DropTable(
                name: "kiosks",
                schema: "kiosk");

            migrationBuilder.DropTable(
                name: "profile_languages",
                schema: "kiosk");

            migrationBuilder.DropTable(
                name: "profiles",
                schema: "kiosk");

            migrationBuilder.DropTable(
                name: "sessions",
                schema: "kiosk");

            migrationBuilder.DropTable(
                name: "theme_tokens",
                schema: "kiosk");

            migrationBuilder.DropTable(
                name: "translations",
                schema: "kiosk");

            migrationBuilder.DropTable(
                name: "welcome_settings",
                schema: "kiosk");
        }
    }
}
