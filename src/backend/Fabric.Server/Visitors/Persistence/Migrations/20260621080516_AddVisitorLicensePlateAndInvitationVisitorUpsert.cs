using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fabric.Server.Visitors.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVisitorLicensePlateAndInvitationVisitorUpsert : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "license_plate",
                schema: "visitors",
                table: "visitors",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.Sql("""
                INSERT INTO visitors.visitors (id, first_name, last_name, email, company, tenant_id, license_plate)
                SELECT DISTINCT ON (i.tenant_id, i.email)
                    i.id,
                    i.first_name,
                    i.last_name,
                    i.email,
                    i.company,
                    i.tenant_id,
                    i.license_plate
                FROM visitors.visit_invitations AS i
                LEFT JOIN visitors.visitors AS v
                    ON v.tenant_id = i.tenant_id
                    AND v.email = i.email
                WHERE v.id IS NULL
                    AND i.visitor_id IS NULL
                ORDER BY i.tenant_id, i.email, i.confirmed_at DESC NULLS LAST, i.id;
                """);

            migrationBuilder.Sql("""
                UPDATE visitors.visit_invitations AS i
                SET visitor_id = v.id
                FROM visitors.visitors AS v
                WHERE i.visitor_id IS NULL
                    AND v.tenant_id = i.tenant_id
                    AND v.email = i.email;
                """);

            migrationBuilder.Sql("""
                WITH latest_license_plate AS (
                    SELECT DISTINCT ON (tenant_id, visitor_id)
                        tenant_id,
                        visitor_id,
                        license_plate
                    FROM visitors.visit_invitations
                    WHERE visitor_id IS NOT NULL
                        AND license_plate IS NOT NULL
                        AND confirmation_status = 'Confirmed'
                    ORDER BY tenant_id, visitor_id, confirmed_at DESC NULLS LAST
                )
                UPDATE visitors.visitors AS v
                SET license_plate = latest_license_plate.license_plate
                FROM latest_license_plate
                WHERE v.tenant_id = latest_license_plate.tenant_id
                    AND v.id = latest_license_plate.visitor_id
                    AND v.license_plate IS NULL;
                """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF to_regclass('reception.expected_arrivals') IS NOT NULL THEN
                        UPDATE reception.expected_arrivals AS arrival
                        SET visitor_id = invitation.visitor_id
                        FROM visitors.visit_invitations AS invitation
                        WHERE arrival.invitation_id = invitation.id
                            AND arrival.visitor_id IS NULL
                            AND invitation.visitor_id IS NOT NULL;
                    END IF;
                END $$;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "visitor_id",
                schema: "visitors",
                table: "visit_invitations",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "visitor_id",
                schema: "visitors",
                table: "visit_invitations",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.DropColumn(
                name: "license_plate",
                schema: "visitors",
                table: "visitors");
        }
    }
}
