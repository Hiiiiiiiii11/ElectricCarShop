using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderRepository.Migrations
{
    /// <inheritdoc />
    public partial class fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Contracts_QuotationId",
                table: "Contracts",
                column: "QuotationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Quotations_QuotationId",
                table: "Contracts",
                column: "QuotationId",
                principalTable: "Quotations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Quotations_QuotationId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_QuotationId",
                table: "Contracts");
        }
    }
}
