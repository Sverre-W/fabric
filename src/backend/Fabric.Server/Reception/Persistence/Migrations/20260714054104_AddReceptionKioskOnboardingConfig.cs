using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabric.Server.Reception.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReceptionKioskOnboardingConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "identity_verification_method",
                schema: "reception",
                table: "reception_kiosks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "require_face_picture",
                schema: "reception",
                table: "reception_kiosks",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "identity_verification_method",
                schema: "reception",
                table: "reception_kiosks");

            migrationBuilder.DropColumn(
                name: "require_face_picture",
                schema: "reception",
                table: "reception_kiosks");
        }
    }
}
