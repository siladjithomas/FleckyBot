using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bot.Migrations
{
    /// <inheritdoc />
    public partial class DBSignupAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Signup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<long>(type: "bigint", nullable: false),
                    EventStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangedBy = table.Column<long>(type: "bigint", nullable: false),
                    ChangedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Signup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SignupAllowedGroup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<long>(type: "bigint", nullable: false),
                    SignupId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignupAllowedGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SignupAllowedGroup_Signup_SignupId",
                        column: x => x.SignupId,
                        principalTable: "Signup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SignupAllowedToEdit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<long>(type: "bigint", nullable: false),
                    SignupId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignupAllowedToEdit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SignupAllowedToEdit_Signup_SignupId",
                        column: x => x.SignupId,
                        principalTable: "Signup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SignupCategory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SignupId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignupCategory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SignupCategory_Signup_SignupId",
                        column: x => x.SignupId,
                        principalTable: "Signup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SignupAttendee",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<long>(type: "bigint", nullable: false),
                    SignupId = table.Column<int>(type: "int", nullable: false),
                    SignupCategoryId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignupAttendee", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SignupAttendee_SignupCategory_SignupCategoryId",
                        column: x => x.SignupCategoryId,
                        principalTable: "SignupCategory",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SignupAttendee_Signup_SignupId",
                        column: x => x.SignupId,
                        principalTable: "Signup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SignupAllowedGroup_SignupId",
                table: "SignupAllowedGroup",
                column: "SignupId");

            migrationBuilder.CreateIndex(
                name: "IX_SignupAllowedToEdit_SignupId",
                table: "SignupAllowedToEdit",
                column: "SignupId");

            migrationBuilder.CreateIndex(
                name: "IX_SignupAttendee_SignupCategoryId",
                table: "SignupAttendee",
                column: "SignupCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SignupAttendee_SignupId",
                table: "SignupAttendee",
                column: "SignupId");

            migrationBuilder.CreateIndex(
                name: "IX_SignupCategory_SignupId",
                table: "SignupCategory",
                column: "SignupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SignupAllowedGroup");

            migrationBuilder.DropTable(
                name: "SignupAllowedToEdit");

            migrationBuilder.DropTable(
                name: "SignupAttendee");

            migrationBuilder.DropTable(
                name: "SignupCategory");

            migrationBuilder.DropTable(
                name: "Signup");
        }
    }
}
