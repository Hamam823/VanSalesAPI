using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VanSalesAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddVanIdToInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VanId",
                table: "Invoices",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_VanId",
                table: "Invoices",
                column: "VanId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Vans_VanId",
                table: "Invoices",
                column: "VanId",
                principalTable: "Vans",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Vans_VanId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_VanId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "VanId",
                table: "Invoices");
        }
    }
}
