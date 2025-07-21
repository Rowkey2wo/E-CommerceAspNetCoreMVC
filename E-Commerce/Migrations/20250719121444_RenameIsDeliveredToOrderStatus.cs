using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_Commerce.Migrations
{
    /// <inheritdoc />
    public partial class RenameIsDeliveredToOrderStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.RenameColumn(
                name: "IsDelivered",
                table: "Transactions",
                newName: "OrderStatus");

            migrationBuilder.AlterColumn<string>(
                name: "OrderStatus",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.Sql(@"
                UPDATE Transactions
                SET OrderStatus = 
                    CASE 
                        WHEN OrderStatus = '1' THEN 'Delivered'
                        ELSE 'Pending'
                    END
            ");
        }



        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.Sql(@"
                UPDATE Transactions
                SET OrderStatus = 
                    CASE 
                        WHEN OrderStatus = 'Delivered' THEN '1'
                        ELSE '0'
                    END
            ");

            
            migrationBuilder.RenameColumn(
                name: "OrderStatus",
                table: "Transactions",
                newName: "IsDelivered");
        }
    }
}
