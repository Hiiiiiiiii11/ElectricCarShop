using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgencyRepository.Migrations
{
    /// <inheritdoc />
    public partial class fixdb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VehicleId",
                table: "TestDrives",
                newName: "VehicleInstanceId");

            migrationBuilder.RenameColumn(
                name: "VehicleId",
                table: "AgencyInventories",
                newName: "VehicleInstanceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VehicleInstanceId",
                table: "TestDrives",
                newName: "VehicleId");

            migrationBuilder.RenameColumn(
                name: "VehicleInstanceId",
                table: "AgencyInventories",
                newName: "VehicleId");
        }
    }
}
