using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderRepository.Migrations
{
    /// <inheritdoc />
    public partial class fixxaddclass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Class",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Class",
                table: "Customers");
        }
    }
}
