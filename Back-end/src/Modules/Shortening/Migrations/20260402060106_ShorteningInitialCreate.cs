using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shortening.Migrations
{
    /// <inheritdoc />
    public partial class ShorteningInitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "shortening");

            migrationBuilder.CreateTable(
                name: "ShortenedUrls",
                schema: "shortening",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    ShortCode = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShortenedUrls", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShortenedUrls_ShortCode",
                schema: "shortening",
                table: "ShortenedUrls",
                column: "ShortCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShortenedUrls_Status_ExpiresAt",
                schema: "shortening",
                table: "ShortenedUrls",
                columns: new[] { "Status", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ShortenedUrls_UserId",
                schema: "shortening",
                table: "ShortenedUrls",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShortenedUrls",
                schema: "shortening");
        }
    }
}
