using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgencyRepository.Migrations
{
    /// <inheritdoc />
    public partial class addContractUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "ContractEndDate",
                table: "AgencyContracts",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "ContractImageUrl",
                table: "AgencyContracts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContractImageUrl",
                table: "AgencyContracts");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ContractEndDate",
                table: "AgencyContracts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}
