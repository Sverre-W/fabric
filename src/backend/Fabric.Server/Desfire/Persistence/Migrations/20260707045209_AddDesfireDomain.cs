using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabric.Server.Desfire.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDesfireDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "desfire");

            migrationBuilder.CreateTable(
                name: "chip_designs",
                schema: "desfire",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SpecificationJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chip_designs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "device_leases",
                schema: "desfire",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DeviceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EncodingRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    AcquiredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReleasedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_device_leases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "encoding_batches",
                schema: "desfire",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TransformationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OriginalInputJson = table.Column<string>(type: "jsonb", nullable: false),
                    NormalizedRowsJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_encoding_batches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "encoding_runs",
                schema: "desfire",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TransformationId = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: true),
                    Kind = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InputJson = table.Column<string>(type: "jsonb", nullable: false),
                    ResolvedVariablesJson = table.Column<string>(type: "jsonb", nullable: false),
                    PlanSummaryJson = table.Column<string>(type: "jsonb", nullable: false),
                    CommandAuditJson = table.Column<string>(type: "jsonb", nullable: false),
                    CardUid = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    HardwareAgentId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeviceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_encoding_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "key_diversification_strategies",
                schema: "desfire",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Algorithm = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    InputsJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_key_diversification_strategies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "key_groups",
                schema: "desfire",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    KeyType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Locked = table.Column<bool>(type: "boolean", nullable: false),
                    DiversificationStrategyId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_key_groups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "transformations",
                schema: "desfire",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FromChipDesignName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FromBlankType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ToChipDesignName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AlwaysReadUid = table.Column<bool>(type: "boolean", nullable: false),
                    RequiredVariablesJson = table.Column<string>(type: "jsonb", nullable: false),
                    RequiredKeyGroupsJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transformations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "key_group_key_sets",
                schema: "desfire",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KeySetId = table.Column<int>(type: "integer", nullable: false),
                    key_group_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_key_group_key_sets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_key_group_key_sets_key_groups_key_group_id",
                        column: x => x.key_group_id,
                        principalSchema: "desfire",
                        principalTable: "key_groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "key_group_keys",
                schema: "desfire",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KeyId = table.Column<int>(type: "integer", nullable: false),
                    ProtectedValue = table.Column<string>(type: "text", nullable: false),
                    IsDiversified = table.Column<bool>(type: "boolean", nullable: false),
                    key_set_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_key_group_keys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_key_group_keys_key_group_key_sets_key_set_id",
                        column: x => x.key_set_id,
                        principalSchema: "desfire",
                        principalTable: "key_group_key_sets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_chip_designs_Name_Version",
                schema: "desfire",
                table: "chip_designs",
                columns: new[] { "Name", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_chip_designs_tenant_id",
                schema: "desfire",
                table: "chip_designs",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_device_leases_AgentId_DeviceId",
                schema: "desfire",
                table: "device_leases",
                columns: new[] { "AgentId", "DeviceId" });

            migrationBuilder.CreateIndex(
                name: "ix_device_leases_tenant_id",
                schema: "desfire",
                table: "device_leases",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_encoding_batches_tenant_id",
                schema: "desfire",
                table: "encoding_batches",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_encoding_batches_TransformationId",
                schema: "desfire",
                table: "encoding_batches",
                column: "TransformationId");

            migrationBuilder.CreateIndex(
                name: "IX_encoding_runs_BatchId",
                schema: "desfire",
                table: "encoding_runs",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_encoding_runs_CardUid",
                schema: "desfire",
                table: "encoding_runs",
                column: "CardUid");

            migrationBuilder.CreateIndex(
                name: "ix_encoding_runs_tenant_id",
                schema: "desfire",
                table: "encoding_runs",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_encoding_runs_TransformationId",
                schema: "desfire",
                table: "encoding_runs",
                column: "TransformationId");

            migrationBuilder.CreateIndex(
                name: "IX_key_diversification_strategies_Name",
                schema: "desfire",
                table: "key_diversification_strategies",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_key_diversification_strategies_tenant_id",
                schema: "desfire",
                table: "key_diversification_strategies",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_key_group_key_sets_key_group_id_KeySetId",
                schema: "desfire",
                table: "key_group_key_sets",
                columns: new[] { "key_group_id", "KeySetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_key_group_keys_key_set_id_KeyId",
                schema: "desfire",
                table: "key_group_keys",
                columns: new[] { "key_set_id", "KeyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_key_groups_Name",
                schema: "desfire",
                table: "key_groups",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_key_groups_tenant_id",
                schema: "desfire",
                table: "key_groups",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_transformations_Name",
                schema: "desfire",
                table: "transformations",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_transformations_tenant_id",
                schema: "desfire",
                table: "transformations",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "chip_designs",
                schema: "desfire");

            migrationBuilder.DropTable(
                name: "device_leases",
                schema: "desfire");

            migrationBuilder.DropTable(
                name: "encoding_batches",
                schema: "desfire");

            migrationBuilder.DropTable(
                name: "encoding_runs",
                schema: "desfire");

            migrationBuilder.DropTable(
                name: "key_diversification_strategies",
                schema: "desfire");

            migrationBuilder.DropTable(
                name: "key_group_keys",
                schema: "desfire");

            migrationBuilder.DropTable(
                name: "transformations",
                schema: "desfire");

            migrationBuilder.DropTable(
                name: "key_group_key_sets",
                schema: "desfire");

            migrationBuilder.DropTable(
                name: "key_groups",
                schema: "desfire");
        }
    }
}
