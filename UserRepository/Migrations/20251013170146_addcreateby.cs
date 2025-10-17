using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserRepository.Migrations
{
    /// <inheritdoc />
    public partial class addcreateby : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Created_By",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Created_By",
                table: "Users");
        }
    }
}
