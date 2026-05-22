using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VanSalesAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPayment22 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SalesmanId",
                table: "Vans",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Salesmen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Salesmen", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vans_SalesmanId",
                table: "Vans",
                column: "SalesmanId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vans_Salesmen_SalesmanId",
                table: "Vans",
                column: "SalesmanId",
                principalTable: "Salesmen",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vans_Salesmen_SalesmanId",
                table: "Vans");

            migrationBuilder.DropTable(
                name: "Salesmen");

            migrationBuilder.DropIndex(
                name: "IX_Vans_SalesmanId",
                table: "Vans");

            migrationBuilder.DropColumn(
                name: "SalesmanId",
                table: "Vans");
        }
    }
}
