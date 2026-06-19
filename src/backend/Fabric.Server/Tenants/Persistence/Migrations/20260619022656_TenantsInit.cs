using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabric.Server.Tenants.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TenantsInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "tenancy");

            migrationBuilder.CreateTable(
                name: "tenants",
                schema: "tenancy",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    oidc_metadata_url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    oidc_client_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    oidc_require_https_metadata = table.Column<bool>(type: "boolean", nullable: false),
                    theme_background_color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false, defaultValue: "#f8f8f8"),
                    theme_content_color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false, defaultValue: "#ffffff"),
                    theme_primary_color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false, defaultValue: "#238cff"),
                    theme_text_color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false, defaultValue: "#212529"),
                    theme_text_muted_color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false, defaultValue: "#6c757d"),
                    theme_border_color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false, defaultValue: "#dddddd"),
                    theme_hover_blue_color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false, defaultValue: "#eef6ff"),
                    theme_active_blue_color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false, defaultValue: "#deeeff"),
                    theme_hover_gray_color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false, defaultValue: "#f3f3f3"),
                    theme_error_color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false, defaultValue: "#ff6467"),
                    theme_error_background_color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false, defaultValue: "#feeaea"),
                    theme_danger_color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false, defaultValue: "#ff6467"),
                    theme_success_color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false, defaultValue: "#00c950"),
                    theme_success_background_color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false, defaultValue: "#e6faeb"),
                    logo_content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    logo_data = table.Column<byte[]>(type: "bytea", maxLength: 1048576, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenants", x => x.id);
                    table.CheckConstraint("ck_tenants_logo_data_max_length", "logo_data IS NULL OR octet_length(logo_data) <= 1048576");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tenants",
                schema: "tenancy");
        }
    }
}
