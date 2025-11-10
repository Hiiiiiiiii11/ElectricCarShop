using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AllocationRepository.Migrations
{
    /// <inheritdoc />
    public partial class addAgencyOrderId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AgencyOrderId",
                table: "Allocations",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgencyOrderId",
                table: "Allocations");
        }
    }
}
