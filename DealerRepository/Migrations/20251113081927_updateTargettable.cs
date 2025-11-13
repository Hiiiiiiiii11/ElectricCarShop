using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgencyRepository.Migrations
{
    /// <inheritdoc />
    public partial class updateTargettable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TargetSales",
                table: "AgencyTargets",
                newName: "VehicleId");

            migrationBuilder.RenameColumn(
                name: "AchievedSales",
                table: "AgencyTargets",
                newName: "TargetUnits");

            migrationBuilder.AddColumn<int>(
                name: "AchievedUnits",
                table: "AgencyTargets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AgencyTargets",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "AgencyTargets",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "OrderDate",
                table: "AgencyOrders",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AchievedUnits",
                table: "AgencyTargets");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AgencyTargets");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AgencyTargets");

            migrationBuilder.DropColumn(
                name: "OrderDate",
                table: "AgencyOrders");

            migrationBuilder.RenameColumn(
                name: "VehicleId",
                table: "AgencyTargets",
                newName: "TargetSales");

            migrationBuilder.RenameColumn(
                name: "TargetUnits",
                table: "AgencyTargets",
                newName: "AchievedSales");
        }
    }
}
