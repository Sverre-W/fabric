using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabric.Server.Desfire.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceTransformationBlankType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "FromBlank",
                schema: "desfire",
                table: "transformations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("UPDATE desfire.transformations SET \"FromBlank\" = TRUE WHERE \"FromBlankType\" IS NOT NULL");

            migrationBuilder.DropColumn(
                name: "FromBlankType",
                schema: "desfire",
                table: "transformations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FromBlankType",
                schema: "desfire",
                table: "transformations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.Sql("UPDATE desfire.transformations SET \"FromBlankType\" = 'AesDefaultPicc' WHERE \"FromBlank\" = TRUE");

            migrationBuilder.DropColumn(
                name: "FromBlank",
                schema: "desfire",
                table: "transformations");
        }
    }
}
