using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HebronPay.Migrations
{
    public partial class createseubaccounts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "subAccountId",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "HebronPayWallets",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    walletBalance = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HebronPayWallets", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "SubAccounts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    account_reference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    account_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    barter_id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    mobilenumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nuban = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    bank_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    bank_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubAccounts", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_subAccountId",
                table: "AspNetUsers",
                column: "subAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_SubAccounts_subAccountId",
                table: "AspNetUsers",
                column: "subAccountId",
                principalTable: "SubAccounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_SubAccounts_subAccountId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "HebronPayWallets");

            migrationBuilder.DropTable(
                name: "SubAccounts");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_subAccountId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "subAccountId",
                table: "AspNetUsers");
        }
    }
}
