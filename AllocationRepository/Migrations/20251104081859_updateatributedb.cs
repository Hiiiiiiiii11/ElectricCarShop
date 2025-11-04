using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AllocationRepository.Migrations
{
    /// <inheritdoc />
    public partial class updateatributedb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AgencyId",
                table: "VehiclePromotions",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgencyId",
                table: "VehiclePromotions");
        }
    }
}
