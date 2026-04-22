using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LaptopServer.Migrations
{
    /// <inheritdoc />
    public partial class MoreLaptopInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Battery",
                table: "Laptops",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiskSize",
                table: "Laptops",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ScreenRefresh",
                table: "Laptops",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ScreenResolution",
                table: "Laptops",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ScreenSize",
                table: "Laptops",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Battery",
                table: "Laptops");

            migrationBuilder.DropColumn(
                name: "DiskSize",
                table: "Laptops");

            migrationBuilder.DropColumn(
                name: "ScreenRefresh",
                table: "Laptops");

            migrationBuilder.DropColumn(
                name: "ScreenResolution",
                table: "Laptops");

            migrationBuilder.DropColumn(
                name: "ScreenSize",
                table: "Laptops");
        }
    }
}
