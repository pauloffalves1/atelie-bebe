using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtelieBebe.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddressCity",
                table: "Customers",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressComplement",
                table: "Customers",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressNeighborhood",
                table: "Customers",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressNumber",
                table: "Customers",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressState",
                table: "Customers",
                type: "TEXT",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressStreet",
                table: "Customers",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressZipCode",
                table: "Customers",
                type: "TEXT",
                maxLength: 9,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressCity",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AddressComplement",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AddressNeighborhood",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AddressNumber",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AddressState",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AddressStreet",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AddressZipCode",
                table: "Customers");
        }
    }
}
