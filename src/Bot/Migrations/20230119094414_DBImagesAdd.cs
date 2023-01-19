using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bot.Migrations
{
    public partial class DBImagesAdd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Uri = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Images",
                columns: new[] { "Id", "Type", "Uri" },
                values: new object[,]
                {
                    { 1, "headpat", "https://img3.gelbooru.com//images/8a/8f/8a8f294a93da9b7e784ca94af437ea96.gif" },
                    { 2, "headpat", "https://img3.gelbooru.com/images/40/6f/406fa299092a50741e958f6931d292c5.gif" },
                    { 3, "headpat", "https://img3.gelbooru.com//images/41/a1/41a100d72145db1ccf9010e2b61ce24f.gif" },
                    { 4, "headpat", "https://img3.gelbooru.com//images/a5/f8/a5f874af32db62ec080ff4af9b2423b8.gif" },
                    { 5, "headpat", "https://img3.gelbooru.com//images/4b/f9/4bf98e334b505bd2c0d1def78eb5d2ef.gif" },
                    { 6, "headpat", "https://img3.gelbooru.com//images/f1/6d/f16d83190cf5405a98445fbafb6f8310.gif" },
                    { 7, "headpat", "https://img3.gelbooru.com//images/6a/49/6a49d8be08214e738b6ce66c55b2d027.gif" },
                    { 8, "headpat", "https://img3.gelbooru.com//images/45/aa/45aa4ae65a38b5da8aa8ae1c04e3818e.gif" },
                    { 9, "headpat", "https://img3.gelbooru.com//images/2e/53/2e535edf467a92c907c478ce53d29548.gif" },
                    { 10, "headpat", "https://img3.gelbooru.com//images/5a/ed/5aed8ada95ba49d416c9bc69550861fa.gif" },
                    { 11, "headpat", "https://img3.gelbooru.com//images/65/45/6545241138ecc15bc0a0a0d0fb2738b6.gif" },
                    { 12, "headpat", "https://img3.gelbooru.com//images/22/16/2216ef6dc3e4dcd405f3ff6376f514f8.gif" },
                    { 13, "headpat", "https://img3.gelbooru.com//images/ab/3f/ab3f4628123bc21f9628bfc6f5b39e58.gif" },
                    { 14, "slap", "https://img3.gelbooru.com//images/05/ba/05ba92b8cbf59eeb2d41ca1a271a2dd0.gif" },
                    { 15, "slap", "https://img3.gelbooru.com//images/59/c0/59c0bc9ce5e51132d0c71820fae345e2.gif" },
                    { 16, "slap", "https://img3.gelbooru.com//images/b2/09/b209bf140ec675ec27294ad8b158477b.gif" },
                    { 17, "slap", "https://img3.gelbooru.com//images/d6/18/d6185516dd7b757b1a6e7437f890cbcd.gif" },
                    { 18, "slap", "https://img3.gelbooru.com//images/45/33/4533dc6aa05c675cb5e83816b90dec58.gif" },
                    { 19, "slap", "https://img3.gelbooru.com/images/33/4b/334bc9cd524d61784c9ad9dbd0a88ea1.gif" },
                    { 20, "slap", "https://img3.gelbooru.com//images/c0/e8/c0e858e3f1a953158cf90945df867819.gif" },
                    { 21, "slap", "https://img3.gelbooru.com//images/d4/3d/d43dbff5e5f01d4e8339caac15e8bdc2.gif" },
                    { 22, "slap", "https://img3.gelbooru.com//images/37/e0/37e09e7d1dac14d481f01dba411b1fa6.gif" },
                    { 23, "slap", "https://img3.gelbooru.com//images/c4/65/c465fb2c1efc66c611e23e7aa199fa89.gif" },
                    { 24, "slap", "https://img3.gelbooru.com//images/82/c7/82c74186db274f5aa82d8067603bd5af.gif" },
                    { 25, "kiss", "https://img3.gelbooru.com//images/45/ff/45ffa2bb5d49ee15af70dfd9f7a59088.gif" },
                    { 26, "kiss", "https://img3.gelbooru.com//images/31/32/313233f5441e8edd979acd0ac48447f6.gif" },
                    { 27, "kiss", "https://img3.gelbooru.com//images/14/db/14db7a05d019091b0c75cc1f6934aa62.gif" },
                    { 28, "kiss", "https://img3.gelbooru.com//images/21/6e/216e01f750334a88ad157b950e03bf3c.gif" },
                    { 29, "kiss", "https://img3.gelbooru.com//images/9f/34/9f34e32731d2a492a9c69ab2d3bdbc33.gif" },
                    { 30, "kiss", "https://img3.gelbooru.com//images/05/e4/05e4229d1ddc90bdbbbe39988d53d127.gif" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Images");
        }
    }
}
