using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabric.Server.Desfire.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDesfireSystemProvider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "system_providers",
                schema: "desfire",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProviderType = table.Column<int>(type: "integer", nullable: false),
                    FixedValue = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    InitialValue = table.Column<long>(type: "bigint", nullable: true),
                    CurrentValue = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_providers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_system_providers_tenant_id",
                schema: "desfire",
                table: "system_providers",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_system_providers_tenant_id_name",
                schema: "desfire",
                table: "system_providers",
                columns: new[] { "tenant_id", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "system_providers",
                schema: "desfire");
        }
    }
}
