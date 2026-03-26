using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiPujas.Migrations
{
    /// <inheritdoc />
    public partial class TotalToPay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TotalToPay",
                table: "Purchases",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalToPay",
                table: "Purchases");
        }
    }
}
