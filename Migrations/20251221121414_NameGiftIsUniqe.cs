using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chinese_sale_api.Migrations
{
    /// <inheritdoc />
    public partial class NameGiftIsUniqe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Gifts_Name",
                table: "Gifts",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Gifts_Name",
                table: "Gifts");
        }
    }
}
