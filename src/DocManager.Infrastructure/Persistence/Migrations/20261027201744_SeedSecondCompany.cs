using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DocManager.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedSecondCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Documents",
                columns: new[] { "Id", "CompanyId", "ContentType", "DateAdded", "DeletedDate", "FileName", "FileSize", "StoredFileName", "Title" },
                values: new object[,]
                {
                    { 1001, 2, "application/pdf", new DateTime(2026, 3, 1, 8, 0, 0, 0, DateTimeKind.Utc), null, "supplier-contract.pdf", 960L, "3f2b8c1e9a4d4c6b8e7f1a2b3c4d5e04.pdf", "Supplier Contract" },
                    { 1002, 2, "image/png", new DateTime(2026, 3, 18, 13, 45, 0, 0, DateTimeKind.Utc), null, "warehouse-layout.png", 2521L, "3f2b8c1e9a4d4c6b8e7f1a2b3c4d5e05.png", "Warehouse Layout" },
                    { 1003, 2, "application/pdf", new DateTime(2026, 5, 20, 16, 0, 0, 0, DateTimeKind.Utc), null, "board-meeting-minutes.pdf", 960L, "3f2b8c1e9a4d4c6b8e7f1a2b3c4d5e06.pdf", "Board Meeting Minutes" },
                    { 1004, 2, "application/pdf", new DateTime(2025, 11, 3, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 15, 12, 0, 0, 0, DateTimeKind.Utc), "price-list-2025.pdf", 821L, "3f2b8c1e9a4d4c6b8e7f1a2b3c4d5e07.pdf", "Price List 2025" }
                });

            migrationBuilder.InsertData(
                table: "DocumentNotes",
                columns: new[] { "Id", "DateAdded", "DocumentId", "NoteText", "PositionLeft", "PositionTop" },
                values: new object[] { 1001, new DateTime(2026, 3, 2, 9, 30, 0, 0, DateTimeKind.Utc), 1001, "Company 2 only: renegotiate payment terms before renewal.", 55m, 30m });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DocumentNotes",
                keyColumn: "Id",
                keyValue: 1001);

            migrationBuilder.DeleteData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 1002);

            migrationBuilder.DeleteData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 1003);

            migrationBuilder.DeleteData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 1004);

            migrationBuilder.DeleteData(
                table: "Documents",
                keyColumn: "Id",
                keyValue: 1001);
        }
    }
}
