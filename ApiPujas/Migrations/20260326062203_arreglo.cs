using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiPujas.Migrations
{
    /// <inheritdoc />
    public partial class arreglo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bids_Products_ProductId1",
                table: "Bids");

            migrationBuilder.DropIndex(
                name: "IX_Bids_ProductId1",
                table: "Bids");

            migrationBuilder.DropColumn(
                name: "ProductId1",
                table: "Bids");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductId1",
                table: "Bids",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bids_ProductId1",
                table: "Bids",
                column: "ProductId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Bids_Products_ProductId1",
                table: "Bids",
                column: "ProductId1",
                principalTable: "Products",
                principalColumn: "Id");
        }
    }
}
