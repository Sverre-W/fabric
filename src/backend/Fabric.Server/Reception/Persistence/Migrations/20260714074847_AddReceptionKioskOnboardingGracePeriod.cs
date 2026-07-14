using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabric.Server.Reception.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReceptionKioskOnboardingGracePeriod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "onboarding_grace_period_minutes",
                schema: "reception",
                table: "reception_kiosks",
                type: "integer",
                nullable: false,
                defaultValue: 60);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "onboarding_grace_period_minutes",
                schema: "reception",
                table: "reception_kiosks");
        }
    }
}
