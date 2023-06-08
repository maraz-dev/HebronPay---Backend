using Microsoft.EntityFrameworkCore.Migrations;

namespace HebronPay.Migrations
{
    public partial class pendingtransactiontype : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "type",
                table: "HebronPayTransactions",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "type",
                table: "HebronPayTransactions");
        }
    }
}
