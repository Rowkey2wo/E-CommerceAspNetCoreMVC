using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_Commerce.Migrations
{
    /// <inheritdoc />
    public partial class AddRiderToTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RiderId",
                table: "Transactions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_RiderId",
                table: "Transactions",
                column: "RiderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Riders_RiderId",
                table: "Transactions",
                column: "RiderId",
                principalTable: "Riders",
                principalColumn: "RiderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Riders_RiderId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_RiderId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "RiderId",
                table: "Transactions");
        }
    }
}
