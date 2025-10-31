using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderRepository.Migrations
{
    /// <inheritdoc />
    public partial class addcontractimg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContractImagageUrl",
                table: "Contracts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContractImagageUrl",
                table: "Contracts");
        }
    }
}
