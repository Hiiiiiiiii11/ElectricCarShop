using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AllocationRepository.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreat1e : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Allocations_EVInventories_EvInventoryId",
                table: "Allocations");

            migrationBuilder.DropColumn(
                name: "AgencyId",
                table: "Allocations");

            migrationBuilder.RenameColumn(
                name: "EvInventoryId",
                table: "Allocations",
                newName: "EVInventoryId");

            migrationBuilder.RenameColumn(
                name: "AllocationQuantity",
                table: "Allocations",
                newName: "AgencyContractId");

            migrationBuilder.RenameIndex(
                name: "IX_Allocations_EvInventoryId",
                table: "Allocations",
                newName: "IX_Allocations_EVInventoryId");

            migrationBuilder.AlterColumn<int>(
                name: "EVInventoryId",
                table: "Allocations",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Allocations_EVInventories_EVInventoryId",
                table: "Allocations",
                column: "EVInventoryId",
                principalTable: "EVInventories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Allocations_EVInventories_EVInventoryId",
                table: "Allocations");

            migrationBuilder.RenameColumn(
                name: "EVInventoryId",
                table: "Allocations",
                newName: "EvInventoryId");

            migrationBuilder.RenameColumn(
                name: "AgencyContractId",
                table: "Allocations",
                newName: "AllocationQuantity");

            migrationBuilder.RenameIndex(
                name: "IX_Allocations_EVInventoryId",
                table: "Allocations",
                newName: "IX_Allocations_EvInventoryId");

            migrationBuilder.AlterColumn<int>(
                name: "EvInventoryId",
                table: "Allocations",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AgencyId",
                table: "Allocations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Allocations_EVInventories_EvInventoryId",
                table: "Allocations",
                column: "EvInventoryId",
                principalTable: "EVInventories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
