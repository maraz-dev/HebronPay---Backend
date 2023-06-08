using Microsoft.EntityFrameworkCore.Migrations;

namespace HebronPay.Migrations
{
    public partial class hebronpaywallet : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "walletPin",
                table: "HebronPayWallets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "hebronPayWalletId",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_hebronPayWalletId",
                table: "AspNetUsers",
                column: "hebronPayWalletId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_HebronPayWallets_hebronPayWalletId",
                table: "AspNetUsers",
                column: "hebronPayWalletId",
                principalTable: "HebronPayWallets",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_HebronPayWallets_hebronPayWalletId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_hebronPayWalletId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "walletPin",
                table: "HebronPayWallets");

            migrationBuilder.DropColumn(
                name: "hebronPayWalletId",
                table: "AspNetUsers");
        }
    }
}
