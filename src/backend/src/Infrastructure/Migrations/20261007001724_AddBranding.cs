using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBranding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "branding",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    app_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    logo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    favicon_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    banner_urls = table.Column<string>(type: "jsonb", nullable: false),
                    primary_light = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    primary = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    primary_dark = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_branding", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "branding");
        }
    }
}
