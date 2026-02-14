using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalFinanceManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "CreditLimit",
                table: "Accounts",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreditLimit",
                table: "Accounts");
        }
    }
}
