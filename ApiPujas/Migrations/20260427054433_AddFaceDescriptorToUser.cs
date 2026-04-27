using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiPujas.Migrations
{
    /// <inheritdoc />
    public partial class AddFaceDescriptorToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FaceDescriptor",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FaceDescriptor",
                table: "Users");
        }
    }
}
