using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VanSalesAPI.Migrations
{
    /// <inheritdoc />
    public partial class CurrencySystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AmountOriginal",
                table: "Payments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmountOriginal",
                table: "Payments");
        }
    }
}
