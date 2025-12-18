using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chinese_sale_api.Migrations
{
    /// <inheritdoc />
    public partial class changeTablesTitles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gift_Category_CategoryId",
                table: "Gift");

            migrationBuilder.DropForeignKey(
                name: "FK_Gift_Donor_DonorId",
                table: "Gift");

            migrationBuilder.DropForeignKey(
                name: "FK_Gift_User_WinnerId",
                table: "Gift");

            migrationBuilder.DropForeignKey(
                name: "FK_Purchase_Gift_GiftId",
                table: "Purchase");

            migrationBuilder.DropForeignKey(
                name: "FK_Purchase_User_CustomerId",
                table: "Purchase");

            migrationBuilder.DropPrimaryKey(
                name: "PK_User",
                table: "User");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Purchase",
                table: "Purchase");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Manager",
                table: "Manager");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Gift",
                table: "Gift");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Donor",
                table: "Donor");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Category",
                table: "Category");

            migrationBuilder.RenameTable(
                name: "User",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "Purchase",
                newName: "Purchases");

            migrationBuilder.RenameTable(
                name: "Manager",
                newName: "Managers");

            migrationBuilder.RenameTable(
                name: "Gift",
                newName: "Gifts");

            migrationBuilder.RenameTable(
                name: "Donor",
                newName: "Donors");

            migrationBuilder.RenameTable(
                name: "Category",
                newName: "Categories");

            migrationBuilder.RenameIndex(
                name: "IX_User_Email",
                table: "Users",
                newName: "IX_Users_Email");

            migrationBuilder.RenameIndex(
                name: "IX_Purchase_GiftId",
                table: "Purchases",
                newName: "IX_Purchases_GiftId");

            migrationBuilder.RenameIndex(
                name: "IX_Purchase_CustomerId",
                table: "Purchases",
                newName: "IX_Purchases_CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_Manager_Email",
                table: "Managers",
                newName: "IX_Managers_Email");

            migrationBuilder.RenameIndex(
                name: "IX_Gift_WinnerId",
                table: "Gifts",
                newName: "IX_Gifts_WinnerId");

            migrationBuilder.RenameIndex(
                name: "IX_Gift_DonorId",
                table: "Gifts",
                newName: "IX_Gifts_DonorId");

            migrationBuilder.RenameIndex(
                name: "IX_Gift_CategoryId",
                table: "Gifts",
                newName: "IX_Gifts_CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Donor_Email",
                table: "Donors",
                newName: "IX_Donors_Email");

            migrationBuilder.RenameIndex(
                name: "IX_Category_Name",
                table: "Categories",
                newName: "IX_Categories_Name");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Purchases",
                table: "Purchases",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Managers",
                table: "Managers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Gifts",
                table: "Gifts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Donors",
                table: "Donors",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Categories",
                table: "Categories",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Gifts_Categories_CategoryId",
                table: "Gifts",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Gifts_Donors_DonorId",
                table: "Gifts",
                column: "DonorId",
                principalTable: "Donors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Gifts_Users_WinnerId",
                table: "Gifts",
                column: "WinnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_Gifts_GiftId",
                table: "Purchases",
                column: "GiftId",
                principalTable: "Gifts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_Users_CustomerId",
                table: "Purchases",
                column: "CustomerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gifts_Categories_CategoryId",
                table: "Gifts");

            migrationBuilder.DropForeignKey(
                name: "FK_Gifts_Donors_DonorId",
                table: "Gifts");

            migrationBuilder.DropForeignKey(
                name: "FK_Gifts_Users_WinnerId",
                table: "Gifts");

            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_Gifts_GiftId",
                table: "Purchases");

            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_Users_CustomerId",
                table: "Purchases");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Purchases",
                table: "Purchases");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Managers",
                table: "Managers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Gifts",
                table: "Gifts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Donors",
                table: "Donors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Categories",
                table: "Categories");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "User");

            migrationBuilder.RenameTable(
                name: "Purchases",
                newName: "Purchase");

            migrationBuilder.RenameTable(
                name: "Managers",
                newName: "Manager");

            migrationBuilder.RenameTable(
                name: "Gifts",
                newName: "Gift");

            migrationBuilder.RenameTable(
                name: "Donors",
                newName: "Donor");

            migrationBuilder.RenameTable(
                name: "Categories",
                newName: "Category");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Email",
                table: "User",
                newName: "IX_User_Email");

            migrationBuilder.RenameIndex(
                name: "IX_Purchases_GiftId",
                table: "Purchase",
                newName: "IX_Purchase_GiftId");

            migrationBuilder.RenameIndex(
                name: "IX_Purchases_CustomerId",
                table: "Purchase",
                newName: "IX_Purchase_CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_Managers_Email",
                table: "Manager",
                newName: "IX_Manager_Email");

            migrationBuilder.RenameIndex(
                name: "IX_Gifts_WinnerId",
                table: "Gift",
                newName: "IX_Gift_WinnerId");

            migrationBuilder.RenameIndex(
                name: "IX_Gifts_DonorId",
                table: "Gift",
                newName: "IX_Gift_DonorId");

            migrationBuilder.RenameIndex(
                name: "IX_Gifts_CategoryId",
                table: "Gift",
                newName: "IX_Gift_CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Donors_Email",
                table: "Donor",
                newName: "IX_Donor_Email");

            migrationBuilder.RenameIndex(
                name: "IX_Categories_Name",
                table: "Category",
                newName: "IX_Category_Name");

            migrationBuilder.AddPrimaryKey(
                name: "PK_User",
                table: "User",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Purchase",
                table: "Purchase",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Manager",
                table: "Manager",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Gift",
                table: "Gift",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Donor",
                table: "Donor",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Category",
                table: "Category",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Gift_Category_CategoryId",
                table: "Gift",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Gift_Donor_DonorId",
                table: "Gift",
                column: "DonorId",
                principalTable: "Donor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Gift_User_WinnerId",
                table: "Gift",
                column: "WinnerId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Purchase_Gift_GiftId",
                table: "Purchase",
                column: "GiftId",
                principalTable: "Gift",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Purchase_User_CustomerId",
                table: "Purchase",
                column: "CustomerId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
