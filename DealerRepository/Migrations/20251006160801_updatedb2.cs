using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgencyRepository.Migrations
{
    /// <inheritdoc />
    public partial class updatedb2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgencyContractId",
                table: "AgencyContracts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AgencyContractId",
                table: "AgencyContracts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
