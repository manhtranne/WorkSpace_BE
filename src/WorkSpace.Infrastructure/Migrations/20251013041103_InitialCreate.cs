using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkSpace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeOnly>(
                name: "CreatedAt",
                table: "Workspaces",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "Workspaces",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "Addresses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Ward",
                table: "Addresses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "SearchQueryHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Ward = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: true),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Participants = table.Column<int>(type: "int", nullable: true),
                    PriceMin = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PriceMax = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AmenityIdsCsv = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    QueryText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ResultsCount = table.Column<int>(type: "int", nullable: true),
                    ClientIp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchQueryHistories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Workspaces_Capacity",
                table: "Workspaces",
                column: "Capacity");

            migrationBuilder.CreateIndex(
                name: "IX_Workspaces_IsFeatured",
                table: "Workspaces",
                column: "IsFeatured");

            migrationBuilder.CreateIndex(
                name: "IX_Workspaces_PricePerHour",
                table: "Workspaces",
                column: "PricePerHour");

            migrationBuilder.CreateIndex(
                name: "IX_Workspaces_Title",
                table: "Workspaces",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_District_Ward",
                table: "Addresses",
                columns: new[] { "District", "Ward" });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_Ward",
                table: "Addresses",
                column: "Ward");

            migrationBuilder.CreateIndex(
                name: "IX_SearchQueryHistories_CreatedAt",
                table: "SearchQueryHistories",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SearchQueryHistories_Date_StartTime_EndTime",
                table: "SearchQueryHistories",
                columns: new[] { "Date", "StartTime", "EndTime" });

            migrationBuilder.CreateIndex(
                name: "IX_SearchQueryHistories_Ward",
                table: "SearchQueryHistories",
                column: "Ward");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SearchQueryHistories");

            migrationBuilder.DropIndex(
                name: "IX_Workspaces_Capacity",
                table: "Workspaces");

            migrationBuilder.DropIndex(
                name: "IX_Workspaces_IsFeatured",
                table: "Workspaces");

            migrationBuilder.DropIndex(
                name: "IX_Workspaces_PricePerHour",
                table: "Workspaces");

            migrationBuilder.DropIndex(
                name: "IX_Workspaces_Title",
                table: "Workspaces");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_District_Ward",
                table: "Addresses");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_Ward",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Workspaces");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "Workspaces");

            migrationBuilder.DropColumn(
                name: "District",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "Ward",
                table: "Addresses");
        }
    }
}
