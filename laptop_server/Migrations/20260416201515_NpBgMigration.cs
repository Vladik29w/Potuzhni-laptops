using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LaptopServer.Migrations
{
    /// <inheritdoc />
    public partial class NpBgMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NpWarehouses",
                columns: table => new
                {
                    Ref = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SettlementRef = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TypeOfWarehouseRef = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NpWarehouses", x => x.Ref);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NpWarehouses");
        }
    }
}
