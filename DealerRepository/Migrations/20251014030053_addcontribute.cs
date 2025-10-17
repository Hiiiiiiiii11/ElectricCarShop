using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgencyRepository.Migrations
{
    /// <inheritdoc />
    public partial class addcontribute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Avatar",
                table: "Agencys",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Agencys",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Avatar",
                table: "Agencys");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Agencys");
        }
    }
}
