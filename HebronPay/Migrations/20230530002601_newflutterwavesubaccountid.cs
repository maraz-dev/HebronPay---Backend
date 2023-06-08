using Microsoft.EntityFrameworkCore.Migrations;

namespace HebronPay.Migrations
{
    public partial class newflutterwavesubaccountid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "flutterwaveSubAccountId",
                table: "SubAccounts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "flutterwaveSubAccountId",
                table: "SubAccounts");
        }
    }
}
