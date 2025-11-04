using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderRepository.Migrations
{
    /// <inheritdoc />
    public partial class fixfeeback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AgencyId",
                table: "Feedbacks",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgencyId",
                table: "Feedbacks");
        }
    }
}
