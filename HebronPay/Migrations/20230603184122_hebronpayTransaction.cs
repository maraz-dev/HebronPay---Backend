using Microsoft.EntityFrameworkCore.Migrations;

namespace HebronPay.Migrations
{
    public partial class hebronpayTransaction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HebronPayTransactions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    amount = table.Column<double>(type: "float", nullable: false),
                    reference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    date = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    time = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hebronPayWalletId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HebronPayTransactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_HebronPayTransactions_HebronPayWallets_hebronPayWalletId",
                        column: x => x.hebronPayWalletId,
                        principalTable: "HebronPayWallets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HebronPayTransactions_hebronPayWalletId",
                table: "HebronPayTransactions",
                column: "hebronPayWalletId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HebronPayTransactions");
        }
    }
}
