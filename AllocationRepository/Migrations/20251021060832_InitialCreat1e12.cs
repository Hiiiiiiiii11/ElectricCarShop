using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AllocationRepository.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreat1e12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Allocations_EVInventories_EVInventoryId",
                table: "Allocations");

            migrationBuilder.DropIndex(
                name: "IX_Allocations_EVInventoryId",
                table: "Allocations");

            migrationBuilder.DropColumn(
                name: "EVInventoryId",
                table: "Allocations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EVInventoryId",
                table: "Allocations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Allocations_EVInventoryId",
                table: "Allocations",
                column: "EVInventoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Allocations_EVInventories_EVInventoryId",
                table: "Allocations",
                column: "EVInventoryId",
                principalTable: "EVInventories",
                principalColumn: "Id");
        }
    }
}
