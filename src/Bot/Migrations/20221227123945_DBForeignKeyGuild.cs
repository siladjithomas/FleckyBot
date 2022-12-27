using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bot.Migrations
{
    /// <inheritdoc />
    public partial class DBForeignKeyGuild : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuildVotesChannels_Guilds_Id",
                table: "GuildVotesChannels");

            migrationBuilder.DropIndex(
                name: "IX_GuildVotesChannels_GuildId",
                table: "GuildVotesChannels");

            migrationBuilder.AddColumn<int>(
                name: "GuildId1",
                table: "GuildVotesChannels",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GuildId1",
                table: "GuildTicketsChannels",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GuildId1",
                table: "GuildSystemMessagesChannels",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GuildId1",
                table: "GuildRolesChannels",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_GuildVotesChannels_GuildId",
                table: "GuildVotesChannels",
                column: "GuildId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GuildVotesChannels_Guilds_Id",
                table: "GuildVotesChannels",
                column: "Id",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuildVotesChannels_Guilds_Id",
                table: "GuildVotesChannels");

            migrationBuilder.DropIndex(
                name: "IX_GuildVotesChannels_GuildId",
                table: "GuildVotesChannels");

            migrationBuilder.DropColumn(
                name: "GuildId1",
                table: "GuildVotesChannels");

            migrationBuilder.DropColumn(
                name: "GuildId1",
                table: "GuildTicketsChannels");

            migrationBuilder.DropColumn(
                name: "GuildId1",
                table: "GuildSystemMessagesChannels");

            migrationBuilder.DropColumn(
                name: "GuildId1",
                table: "GuildRolesChannels");

            migrationBuilder.CreateIndex(
                name: "IX_GuildVotesChannels_GuildId",
                table: "GuildVotesChannels",
                column: "GuildId");

            migrationBuilder.AddForeignKey(
                name: "FK_GuildVotesChannels_Guilds_Id",
                table: "GuildVotesChannels",
                column: "Id",
                principalTable: "Guilds",
                principalColumn: "Id");
        }
    }
}
