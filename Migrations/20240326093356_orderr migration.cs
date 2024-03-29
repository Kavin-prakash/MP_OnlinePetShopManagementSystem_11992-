using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlinePetShopManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class orderrmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orderdetails",
                columns: table => new
                {
                    OrderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UsersId = table.Column<int>(type: "int", nullable: false),
                    AccessoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orderdetails", x => x.OrderID);
                    table.ForeignKey(
                        name: "FK_Orderdetails_Accessory_AccessoryId",
                        column: x => x.AccessoryId,
                        principalTable: "Accessory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orderdetails_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Orderdetails_AccessoryId",
                table: "Orderdetails",
                column: "AccessoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Orderdetails_UsersId",
                table: "Orderdetails",
                column: "UsersId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Orderdetails");
        }
    }
}
