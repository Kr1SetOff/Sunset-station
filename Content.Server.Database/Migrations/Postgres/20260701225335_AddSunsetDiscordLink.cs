using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class AddSunsetDiscordLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sunset_discord_link",
                columns: table => new
                {
                    player_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    discord_user_id = table.Column<string>(type: "text", nullable: false),
                    linked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    sponsor_tier = table.Column<int>(type: "integer", nullable: false),
                    tier_checked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sunset_discord_link", x => x.player_user_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_sunset_discord_link_discord_user_id",
                table: "sunset_discord_link",
                column: "discord_user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sunset_discord_link");
        }
    }
}
