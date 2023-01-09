using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bot.Migrations
{
    /// <inheritdoc />
    public partial class DBVotesIsOpen : Migration
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

            migrationBuilder.AddColumn<bool>(
                name: "isOpen",
                table: "Vote",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

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
                name: "isOpen",
                table: "Vote");

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
    }
}
