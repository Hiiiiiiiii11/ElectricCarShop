using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnalyticRepository.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EtlLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LastSuccessfulRun = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsRunning = table.Column<bool>(type: "bit", nullable: false),
                    LastStatusMessage = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EtlLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MonthlyDemandFeatures",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    AgencyId = table.Column<int>(type: "int", nullable: false),
                    UnitsSold = table.Column<int>(type: "int", nullable: false),
                    UnitsSoldLastMonth = table.Column<int>(type: "int", nullable: false),
                    UnitsSoldSameMonthLastYear = table.Column<int>(type: "int", nullable: false),
                    RollingAvgSales3Months = table.Column<double>(type: "float", nullable: false),
                    RollingAvgSales6Months = table.Column<double>(type: "float", nullable: false),
                    AvgPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WasOnPromotion = table.Column<bool>(type: "bit", nullable: false),
                    PromotionDiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TestDrivesCount = table.Column<int>(type: "int", nullable: false),
                    QuotationsAcceptedCount = table.Column<int>(type: "int", nullable: false),
                    AgencyOrdersQuantity = table.Column<int>(type: "int", nullable: false),
                    AgencyTarget = table.Column<int>(type: "int", nullable: false),
                    VehicleBatteryCapacity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VehicleRangeKM = table.Column<int>(type: "int", nullable: false),
                    AgencyRegion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyDemandFeatures", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "EtlLogs",
                columns: new[] { "Id", "IsRunning", "LastStatusMessage", "LastSuccessfulRun" },
                values: new object[] { 1, false, "Service starting.", null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EtlLogs");

            migrationBuilder.DropTable(
                name: "MonthlyDemandFeatures");
        }
    }
}
