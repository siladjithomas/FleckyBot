using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bot.Migrations
{
    /// <inheritdoc />
    public partial class DBTelegramUserAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TelegramUser",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramUser", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TelegramChat",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChatId = table.Column<long>(type: "bigint", nullable: true),
                    ChatUsername = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChatBio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChatDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChatType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramChat", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TelegramChat_TelegramUser_UserId",
                        column: x => x.UserId,
                        principalTable: "TelegramUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TelegramMessage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChatId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramMessage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TelegramMessage_TelegramChat_ChatId",
                        column: x => x.ChatId,
                        principalTable: "TelegramChat",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TelegramChat_UserId",
                table: "TelegramChat",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TelegramMessage_ChatId",
                table: "TelegramMessage",
                column: "ChatId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TelegramMessage");

            migrationBuilder.DropTable(
                name: "TelegramChat");

            migrationBuilder.DropTable(
                name: "TelegramUser");
        }
    }
}
