using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bot.Migrations
{
    /// <inheritdoc />
    public partial class InitialiseDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GuildRolesChannels",
                columns: table => new
                {
                    ChannelId = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChannelName = table.Column<string>(type: "TEXT", nullable: false),
                    GuildId = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildRolesChannels", x => x.ChannelId);
                });

            migrationBuilder.CreateTable(
                name: "GuildSystemMessagesChannels",
                columns: table => new
                {
                    ChannelId = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChannelName = table.Column<string>(type: "TEXT", nullable: false),
                    GuildId = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildSystemMessagesChannels", x => x.ChannelId);
                });

            migrationBuilder.CreateTable(
                name: "GuildTicketsChannels",
                columns: table => new
                {
                    ChannelId = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChannelName = table.Column<string>(type: "TEXT", nullable: false),
                    GuildId = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildTicketsChannels", x => x.ChannelId);
                });

            migrationBuilder.CreateTable(
                name: "GuildVotesChannels",
                columns: table => new
                {
                    ChannelId = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChannelName = table.Column<string>(type: "TEXT", nullable: false),
                    GuildId = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildVotesChannels", x => x.ChannelId);
                });

            migrationBuilder.CreateTable(
                name: "RequestableRoles",
                columns: table => new
                {
                    RoleId = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleName = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    GuildId = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestableRoles", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "GuildTicketsGroups",
                columns: table => new
                {
                    ChannelId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    GroupId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    GroupName = table.Column<string>(type: "TEXT", nullable: false),
                    GroupType = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildTicketsGroups", x => x.ChannelId);
                    table.ForeignKey(
                        name: "FK_GuildTicketsGroups_GuildTicketsChannels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "GuildTicketsChannels",
                        principalColumn: "ChannelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    GuildName = table.Column<string>(type: "TEXT", nullable: false),
                    GuildAdminId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    GuildAdminName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.GuildId);
                    table.ForeignKey(
                        name: "FK_Guilds_GuildRolesChannels_GuildId",
                        column: x => x.GuildId,
                        principalTable: "GuildRolesChannels",
                        principalColumn: "ChannelId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Guilds_GuildSystemMessagesChannels_GuildId",
                        column: x => x.GuildId,
                        principalTable: "GuildSystemMessagesChannels",
                        principalColumn: "ChannelId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Guilds_GuildTicketsChannels_GuildId",
                        column: x => x.GuildId,
                        principalTable: "GuildTicketsChannels",
                        principalColumn: "ChannelId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Guilds_GuildVotesChannels_GuildId",
                        column: x => x.GuildId,
                        principalTable: "GuildVotesChannels",
                        principalColumn: "ChannelId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Guilds");

            migrationBuilder.DropTable(
                name: "GuildTicketsGroups");

            migrationBuilder.DropTable(
                name: "RequestableRoles");

            migrationBuilder.DropTable(
                name: "GuildRolesChannels");

            migrationBuilder.DropTable(
                name: "GuildSystemMessagesChannels");

            migrationBuilder.DropTable(
                name: "GuildVotesChannels");

            migrationBuilder.DropTable(
                name: "GuildTicketsChannels");
        }
    }
}
