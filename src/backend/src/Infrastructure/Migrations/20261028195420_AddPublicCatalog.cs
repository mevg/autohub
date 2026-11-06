using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "VehiclePublicReferenceSequence");

            migrationBuilder.AddColumn<string>(
                name: "PublicReference",
                table: "Vehicles",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            // Backfill de vehículos existentes antes de aplicar NOT NULL + UNIQUE.
            migrationBuilder.Sql(
                """
                UPDATE "Vehicles"
                SET "PublicReference" = 'AH-' || lpad(nextval('"VehiclePublicReferenceSequence"')::text, 6, '0')
                WHERE "PublicReference" IS NULL;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "PublicReference",
                table: "Vehicles",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.Sql(
                """
                ALTER TABLE "Vehicles"
                ALTER COLUMN "PublicReference"
                SET DEFAULT 'AH-' || lpad(nextval('"VehiclePublicReferenceSequence"')::text, 6, '0');
                """);

            migrationBuilder.AddColumn<string>(
                name: "AltText",
                table: "VehicleImages",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Leads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ConsentAccepted = table.Column<bool>(type: "boolean", nullable: false),
                    IpHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ContactedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Leads_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_PublicReference",
                table: "Vehicles",
                column: "PublicReference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Leads_Email",
                table: "Leads",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_IdempotencyKey",
                table: "Leads",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_Status_CreatedAt",
                table: "Leads",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Leads_VehicleId_CreatedAt",
                table: "Leads",
                columns: new[] { "VehicleId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_PublicReference",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "PublicReference",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "AltText",
                table: "VehicleImages");

            migrationBuilder.DropSequence(
                name: "VehiclePublicReferenceSequence");
        }
    }
}
