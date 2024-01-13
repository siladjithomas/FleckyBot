using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bot.Migrations
{
    /// <inheritdoc />
    public partial class DBSleepTableRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SleepCallActiveChannel_SleepCallCategory_SleepCallCategoryId",
                table: "SleepCallActiveChannel");

            migrationBuilder.DropForeignKey(
                name: "FK_SleepCallCategory_Guilds_GuildId",
                table: "SleepCallCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_SleepCallGroup_SleepCallCategory_SleepCallCategoryId",
                table: "SleepCallGroup");

            migrationBuilder.DropForeignKey(
                name: "FK_SleepCallIgnoredChannel_Guilds_GuildId",
                table: "SleepCallIgnoredChannel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SleepCallIgnoredChannel",
                table: "SleepCallIgnoredChannel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SleepCallGroup",
                table: "SleepCallGroup");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SleepCallCategory",
                table: "SleepCallCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SleepCallActiveChannel",
                table: "SleepCallActiveChannel");

            migrationBuilder.RenameTable(
                name: "SleepCallIgnoredChannel",
                newName: "SleepCallIgnoredChannels");

            migrationBuilder.RenameTable(
                name: "SleepCallGroup",
                newName: "SleepCallGroups");

            migrationBuilder.RenameTable(
                name: "SleepCallCategory",
                newName: "SleepCallCategorys");

            migrationBuilder.RenameTable(
                name: "SleepCallActiveChannel",
                newName: "SleepCallActiveChannels");

            migrationBuilder.RenameIndex(
                name: "IX_SleepCallIgnoredChannel_GuildId",
                table: "SleepCallIgnoredChannels",
                newName: "IX_SleepCallIgnoredChannels_GuildId");

            migrationBuilder.RenameIndex(
                name: "IX_SleepCallGroup_SleepCallCategoryId",
                table: "SleepCallGroups",
                newName: "IX_SleepCallGroups_SleepCallCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_SleepCallCategory_GuildId",
                table: "SleepCallCategorys",
                newName: "IX_SleepCallCategorys_GuildId");

            migrationBuilder.RenameIndex(
                name: "IX_SleepCallActiveChannel_SleepCallCategoryId",
                table: "SleepCallActiveChannels",
                newName: "IX_SleepCallActiveChannels_SleepCallCategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SleepCallIgnoredChannels",
                table: "SleepCallIgnoredChannels",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SleepCallGroups",
                table: "SleepCallGroups",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SleepCallCategorys",
                table: "SleepCallCategorys",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SleepCallActiveChannels",
                table: "SleepCallActiveChannels",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SleepCallActiveChannels_SleepCallCategorys_SleepCallCategoryId",
                table: "SleepCallActiveChannels",
                column: "SleepCallCategoryId",
                principalTable: "SleepCallCategorys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SleepCallCategorys_Guilds_GuildId",
                table: "SleepCallCategorys",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SleepCallGroups_SleepCallCategorys_SleepCallCategoryId",
                table: "SleepCallGroups",
                column: "SleepCallCategoryId",
                principalTable: "SleepCallCategorys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SleepCallIgnoredChannels_Guilds_GuildId",
                table: "SleepCallIgnoredChannels",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SleepCallActiveChannels_SleepCallCategorys_SleepCallCategoryId",
                table: "SleepCallActiveChannels");

            migrationBuilder.DropForeignKey(
                name: "FK_SleepCallCategorys_Guilds_GuildId",
                table: "SleepCallCategorys");

            migrationBuilder.DropForeignKey(
                name: "FK_SleepCallGroups_SleepCallCategorys_SleepCallCategoryId",
                table: "SleepCallGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_SleepCallIgnoredChannels_Guilds_GuildId",
                table: "SleepCallIgnoredChannels");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SleepCallIgnoredChannels",
                table: "SleepCallIgnoredChannels");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SleepCallGroups",
                table: "SleepCallGroups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SleepCallCategorys",
                table: "SleepCallCategorys");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SleepCallActiveChannels",
                table: "SleepCallActiveChannels");

            migrationBuilder.RenameTable(
                name: "SleepCallIgnoredChannels",
                newName: "SleepCallIgnoredChannel");

            migrationBuilder.RenameTable(
                name: "SleepCallGroups",
                newName: "SleepCallGroup");

            migrationBuilder.RenameTable(
                name: "SleepCallCategorys",
                newName: "SleepCallCategory");

            migrationBuilder.RenameTable(
                name: "SleepCallActiveChannels",
                newName: "SleepCallActiveChannel");

            migrationBuilder.RenameIndex(
                name: "IX_SleepCallIgnoredChannels_GuildId",
                table: "SleepCallIgnoredChannel",
                newName: "IX_SleepCallIgnoredChannel_GuildId");

            migrationBuilder.RenameIndex(
                name: "IX_SleepCallGroups_SleepCallCategoryId",
                table: "SleepCallGroup",
                newName: "IX_SleepCallGroup_SleepCallCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_SleepCallCategorys_GuildId",
                table: "SleepCallCategory",
                newName: "IX_SleepCallCategory_GuildId");

            migrationBuilder.RenameIndex(
                name: "IX_SleepCallActiveChannels_SleepCallCategoryId",
                table: "SleepCallActiveChannel",
                newName: "IX_SleepCallActiveChannel_SleepCallCategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SleepCallIgnoredChannel",
                table: "SleepCallIgnoredChannel",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SleepCallGroup",
                table: "SleepCallGroup",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SleepCallCategory",
                table: "SleepCallCategory",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SleepCallActiveChannel",
                table: "SleepCallActiveChannel",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SleepCallActiveChannel_SleepCallCategory_SleepCallCategoryId",
                table: "SleepCallActiveChannel",
                column: "SleepCallCategoryId",
                principalTable: "SleepCallCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SleepCallCategory_Guilds_GuildId",
                table: "SleepCallCategory",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SleepCallGroup_SleepCallCategory_SleepCallCategoryId",
                table: "SleepCallGroup",
                column: "SleepCallCategoryId",
                principalTable: "SleepCallCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SleepCallIgnoredChannel_Guilds_GuildId",
                table: "SleepCallIgnoredChannel",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
