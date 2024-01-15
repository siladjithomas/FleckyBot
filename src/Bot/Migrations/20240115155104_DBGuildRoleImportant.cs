using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bot.Migrations
{
    /// <inheritdoc />
    public partial class DBGuildRoleImportant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuildRoles_Guilds_GuildId",
                table: "GuildRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GuildRoles",
                table: "GuildRoles");

            migrationBuilder.DropColumn(
                name: "RoleImportance",
                table: "GuildRoles");

            migrationBuilder.RenameTable(
                name: "GuildRoles",
                newName: "ImportantGuildRoles");

            migrationBuilder.RenameIndex(
                name: "IX_GuildRoles_GuildId",
                table: "ImportantGuildRoles",
                newName: "IX_ImportantGuildRoles_GuildId");

            migrationBuilder.AddColumn<string>(
                name: "RoleDescription",
                table: "ImportantGuildRoles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ImportantGuildRoles",
                table: "ImportantGuildRoles",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ImportantGuildRoles_Guilds_GuildId",
                table: "ImportantGuildRoles",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImportantGuildRoles_Guilds_GuildId",
                table: "ImportantGuildRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ImportantGuildRoles",
                table: "ImportantGuildRoles");

            migrationBuilder.DropColumn(
                name: "RoleDescription",
                table: "ImportantGuildRoles");

            migrationBuilder.RenameTable(
                name: "ImportantGuildRoles",
                newName: "GuildRoles");

            migrationBuilder.RenameIndex(
                name: "IX_ImportantGuildRoles_GuildId",
                table: "GuildRoles",
                newName: "IX_GuildRoles_GuildId");

            migrationBuilder.AddColumn<int>(
                name: "RoleImportance",
                table: "GuildRoles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_GuildRoles",
                table: "GuildRoles",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GuildRoles_Guilds_GuildId",
                table: "GuildRoles",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
