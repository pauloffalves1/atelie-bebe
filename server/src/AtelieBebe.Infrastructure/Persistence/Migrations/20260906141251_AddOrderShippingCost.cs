using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtelieBebe.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderShippingCost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ShippingCostAmount",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValueSql: "0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippingCostAmount",
                table: "Orders");
        }
    }
}
