using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtelieBebe.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerCpf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cpf",
                table: "Customers",
                type: "TEXT",
                maxLength: 11,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Cpf",
                table: "Customers",
                column: "Cpf",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customers_Cpf",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Cpf",
                table: "Customers");
        }
    }
}
