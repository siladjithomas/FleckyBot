using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bot.Migrations
{
    /// <inheritdoc />
    public partial class DBGuildTicketTypo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuildTicketsGroups_GuildTicketsChannels_Id",
                table: "GuildTicketsGroups");

            migrationBuilder.AddForeignKey(
                name: "FK_GuildTicketsGroups_GuildTicketsChannels_Id",
                table: "GuildTicketsGroups",
                column: "Id",
                principalTable: "GuildTicketsChannels",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuildTicketsGroups_GuildTicketsChannels_Id",
                table: "GuildTicketsGroups");

            migrationBuilder.AddForeignKey(
                name: "FK_GuildTicketsGroups_GuildTicketsChannels_Id",
                table: "GuildTicketsGroups",
                column: "Id",
                principalTable: "GuildTicketsChannels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
