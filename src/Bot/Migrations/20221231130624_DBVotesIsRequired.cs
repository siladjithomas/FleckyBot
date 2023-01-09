using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bot.Migrations
{
    /// <inheritdoc />
    public partial class DBVotesIsRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VoteUser_Vote_Id",
                table: "VoteUser");

            migrationBuilder.AddForeignKey(
                name: "FK_VoteUser_Vote_Id",
                table: "VoteUser",
                column: "Id",
                principalTable: "Vote",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VoteUser_Vote_Id",
                table: "VoteUser");

            migrationBuilder.AddForeignKey(
                name: "FK_VoteUser_Vote_Id",
                table: "VoteUser",
                column: "Id",
                principalTable: "Vote",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
