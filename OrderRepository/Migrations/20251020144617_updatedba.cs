using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderRepository.Migrations
{
    /// <inheritdoc />
    public partial class updatedba : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Quotations_QuotationId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Quotations_QuotationId1",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_QuotationId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_QuotationId1",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "QuotationId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "QuotationId1",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "Comment",
                table: "Feedbacks",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "SignedBy",
                table: "Contracts",
                newName: "Status");

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Quotations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Feedbacks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Reply",
                table: "Feedbacks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "OrderDetail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    QuotationId = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderDetail_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderDetail_Quotations_QuotationId",
                        column: x => x.QuotationId,
                        principalTable: "Quotations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetail_OrderId",
                table: "OrderDetail",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetail_QuotationId",
                table: "OrderDetail",
                column: "QuotationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderDetail");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "Reply",
                table: "Feedbacks");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Feedbacks",
                newName: "Comment");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Contracts",
                newName: "SignedBy");

            migrationBuilder.AddColumn<int>(
                name: "QuotationId",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuotationId1",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_QuotationId",
                table: "Orders",
                column: "QuotationId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_QuotationId1",
                table: "Orders",
                column: "QuotationId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Quotations_QuotationId",
                table: "Orders",
                column: "QuotationId",
                principalTable: "Quotations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Quotations_QuotationId1",
                table: "Orders",
                column: "QuotationId1",
                principalTable: "Quotations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
