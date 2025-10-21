using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderRepository.Migrations
{
    /// <inheritdoc />
    public partial class updatedbaaa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "Quotations",
                newName: "CreateBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreateBy",
                table: "Quotations",
                newName: "CreatedBy");
        }
    }
}
