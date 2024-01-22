using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bot.Migrations
{
    /// <inheritdoc />
    public partial class DbGuildTimetableChannel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GuildTimetableChannel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    TimetableListMessageId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    RequestAppointmentMessageId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    ChannelId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    ChannelName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildTimetableChannel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuildTimetableChannel_Guilds_Id",
                        column: x => x.Id,
                        principalTable: "Guilds",
                        principalColumn: "Id");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuildTimetableChannel");
        }
    }
}
