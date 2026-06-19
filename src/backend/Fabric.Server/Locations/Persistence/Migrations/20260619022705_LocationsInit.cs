using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabric.Server.Locations.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LocationsInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "locations");

            migrationBuilder.CreateTable(
                name: "location_lookup",
                schema: "locations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    site_id = table.Column<Guid>(type: "uuid", nullable: false),
                    building_id = table.Column<Guid>(type: "uuid", nullable: true),
                    room_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_location_lookup", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sites",
                schema: "locations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sites", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "buildings",
                schema: "locations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    site_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_buildings", x => x.id);
                    table.ForeignKey(
                        name: "fk_buildings_sites_site_id",
                        column: x => x.site_id,
                        principalSchema: "locations",
                        principalTable: "sites",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rooms",
                schema: "locations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    capacity = table.Column<int>(type: "integer", nullable: false),
                    wheelchair_accessible = table.Column<bool>(type: "boolean", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    building_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rooms", x => x.id);
                    table.ForeignKey(
                        name: "fk_rooms_buildings_building_id",
                        column: x => x.building_id,
                        principalSchema: "locations",
                        principalTable: "buildings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_buildings_site_id",
                schema: "locations",
                table: "buildings",
                column: "site_id");

            migrationBuilder.CreateIndex(
                name: "ix_buildings_tenant_id",
                schema: "locations",
                table: "buildings",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_buildings_tenant_id_site_id_name",
                schema: "locations",
                table: "buildings",
                columns: new[] { "tenant_id", "site_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_location_lookup_building_id",
                schema: "locations",
                table: "location_lookup",
                column: "building_id");

            migrationBuilder.CreateIndex(
                name: "ix_location_lookup_room_id",
                schema: "locations",
                table: "location_lookup",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "ix_location_lookup_site_id",
                schema: "locations",
                table: "location_lookup",
                column: "site_id");

            migrationBuilder.CreateIndex(
                name: "ix_location_lookup_tenant_id",
                schema: "locations",
                table: "location_lookup",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_rooms_building_id",
                schema: "locations",
                table: "rooms",
                column: "building_id");

            migrationBuilder.CreateIndex(
                name: "ix_rooms_tenant_id",
                schema: "locations",
                table: "rooms",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_rooms_tenant_id_building_id_name",
                schema: "locations",
                table: "rooms",
                columns: new[] { "tenant_id", "building_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sites_tenant_id",
                schema: "locations",
                table: "sites",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "location_lookup",
                schema: "locations");

            migrationBuilder.DropTable(
                name: "rooms",
                schema: "locations");

            migrationBuilder.DropTable(
                name: "buildings",
                schema: "locations");

            migrationBuilder.DropTable(
                name: "sites",
                schema: "locations");
        }
    }
}
