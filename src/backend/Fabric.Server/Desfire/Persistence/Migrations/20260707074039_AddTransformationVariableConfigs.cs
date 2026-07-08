using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabric.Server.Desfire.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTransformationVariableConfigs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VariableConfigsJson",
                schema: "desfire",
                table: "transformations",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VariableConfigsJson",
                schema: "desfire",
                table: "transformations");
        }
    }
}
