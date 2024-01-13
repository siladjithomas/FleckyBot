using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bot.Migrations
{
    /// <inheritdoc />
    public partial class DBGuildRoleRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuildRole_Guilds_GuildId",
                table: "GuildRole");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GuildRole",
                table: "GuildRole");

            migrationBuilder.RenameTable(
                name: "GuildRole",
                newName: "GuildRoles");

            migrationBuilder.RenameIndex(
                name: "IX_GuildRole_GuildId",
                table: "GuildRoles",
                newName: "IX_GuildRoles_GuildId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuildRoles_Guilds_GuildId",
                table: "GuildRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GuildRoles",
                table: "GuildRoles");

            migrationBuilder.RenameTable(
                name: "GuildRoles",
                newName: "GuildRole");

            migrationBuilder.RenameIndex(
                name: "IX_GuildRoles_GuildId",
                table: "GuildRole",
                newName: "IX_GuildRole_GuildId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GuildRole",
                table: "GuildRole",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GuildRole_Guilds_GuildId",
                table: "GuildRole",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
