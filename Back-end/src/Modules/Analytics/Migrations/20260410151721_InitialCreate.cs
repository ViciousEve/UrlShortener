using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Analytics.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "analytics");

            migrationBuilder.CreateTable(
                name: "ShortenedUrlStats",
                schema: "analytics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShortCode = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    OriginalUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    TotalClicks = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LastClickedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShortenedUrlStats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClickEvents",
                schema: "analytics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShortenedUrlStatsId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClickedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClickEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClickEvents_ShortenedUrlStats_ShortenedUrlStatsId",
                        column: x => x.ShortenedUrlStatsId,
                        principalSchema: "analytics",
                        principalTable: "ShortenedUrlStats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClickEvents_ShortenedUrlStatsId",
                schema: "analytics",
                table: "ClickEvents",
                column: "ShortenedUrlStatsId");

            migrationBuilder.CreateIndex(
                name: "IX_ClickEvents_ShortenedUrlStatsId_ClickedAtUtc",
                schema: "analytics",
                table: "ClickEvents",
                columns: new[] { "ShortenedUrlStatsId", "ClickedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ShortenedUrlStats_ShortCode",
                schema: "analytics",
                table: "ShortenedUrlStats",
                column: "ShortCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShortenedUrlStats_UserId",
                schema: "analytics",
                table: "ShortenedUrlStats",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClickEvents",
                schema: "analytics");

            migrationBuilder.DropTable(
                name: "ShortenedUrlStats",
                schema: "analytics");
        }
    }
}
