using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bot.Migrations
{
    /// <inheritdoc />
    public partial class DBSleepyTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SleepCallCategory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    CategoryName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    GuildId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SleepCallCategory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SleepCallCategory_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SleepCallIgnoredChannel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChannelId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    ChannelName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    GuildId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SleepCallIgnoredChannel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SleepCallIgnoredChannel_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SleepCallActiveChannel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChannelId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    ChannelName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    SleepCallCategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SleepCallActiveChannel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SleepCallActiveChannel_SleepCallCategory_SleepCallCategoryId",
                        column: x => x.SleepCallCategoryId,
                        principalTable: "SleepCallCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SleepCallGroup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    SleepCallCategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SleepCallGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SleepCallGroup_SleepCallCategory_SleepCallCategoryId",
                        column: x => x.SleepCallCategoryId,
                        principalTable: "SleepCallCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SleepCallActiveChannel_SleepCallCategoryId",
                table: "SleepCallActiveChannel",
                column: "SleepCallCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SleepCallCategory_GuildId",
                table: "SleepCallCategory",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_SleepCallGroup_SleepCallCategoryId",
                table: "SleepCallGroup",
                column: "SleepCallCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SleepCallIgnoredChannel_GuildId",
                table: "SleepCallIgnoredChannel",
                column: "GuildId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SleepCallActiveChannel");

            migrationBuilder.DropTable(
                name: "SleepCallGroup");

            migrationBuilder.DropTable(
                name: "SleepCallIgnoredChannel");

            migrationBuilder.DropTable(
                name: "SleepCallCategory");
        }
    }
}
