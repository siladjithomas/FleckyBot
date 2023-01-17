using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bot.Migrations
{
    public partial class DBCascadeAdd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuildTicketsGroups_GuildTicketsChannels_GuildTicketsChannelId",
                table: "GuildTicketsGroups");

            migrationBuilder.AlterColumn<int>(
                name: "GuildTicketsChannelId",
                table: "GuildTicketsGroups",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_GuildTicketsGroups_GuildTicketsChannels_GuildTicketsChannelId",
                table: "GuildTicketsGroups",
                column: "GuildTicketsChannelId",
                principalTable: "GuildTicketsChannels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuildTicketsGroups_GuildTicketsChannels_GuildTicketsChannelId",
                table: "GuildTicketsGroups");

            migrationBuilder.AlterColumn<int>(
                name: "GuildTicketsChannelId",
                table: "GuildTicketsGroups",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GuildTicketsGroups_GuildTicketsChannels_GuildTicketsChannelId",
                table: "GuildTicketsGroups",
                column: "GuildTicketsChannelId",
                principalTable: "GuildTicketsChannels",
                principalColumn: "Id");
        }
    }
}
