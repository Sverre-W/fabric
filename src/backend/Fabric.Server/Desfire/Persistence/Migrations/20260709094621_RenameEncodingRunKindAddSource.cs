using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabric.Server.Desfire.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameEncodingRunKindAddSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE desfire.encoding_runs
                SET "Kind" = CASE "Kind"
                    WHEN 'AdHoc' THEN 'Single'
                    WHEN 'BatchItem' THEN 'Batch'
                    ELSE "Kind"
                END;
                """);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                schema: "desfire",
                table: "encoding_runs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_encoding_runs_Source",
                schema: "desfire",
                table: "encoding_runs",
                column: "Source");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_encoding_runs_Source",
                schema: "desfire",
                table: "encoding_runs");

            migrationBuilder.DropColumn(
                name: "Source",
                schema: "desfire",
                table: "encoding_runs");

            migrationBuilder.Sql("""
                UPDATE desfire.encoding_runs
                SET "Kind" = CASE "Kind"
                    WHEN 'Single' THEN 'AdHoc'
                    WHEN 'Batch' THEN 'BatchItem'
                    ELSE "Kind"
                END;
                """);
        }
    }
}
