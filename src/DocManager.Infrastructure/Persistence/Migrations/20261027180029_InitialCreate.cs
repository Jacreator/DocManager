using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DocManager.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    NoteText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    PositionTop = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    PositionLeft = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentNotes_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Documents",
                columns: new[] { "Id", "CompanyId", "ContentType", "DateAdded", "DeletedDate", "FileName", "FileSize", "StoredFileName", "Title" },
                values: new object[,]
                {
                    { 1, 1, "application/pdf", new DateTime(2026, 1, 5, 9, 0, 0, 0, DateTimeKind.Utc), null, "employee-handbook.pdf", 1137L, "3f2b8c1e9a4d4c6b8e7f1a2b3c4d5e01.pdf", "Employee Handbook" },
                    { 2, 1, "image/png", new DateTime(2026, 2, 10, 14, 30, 0, 0, DateTimeKind.Utc), null, "office-floor-plan.png", 3128L, "3f2b8c1e9a4d4c6b8e7f1a2b3c4d5e02.png", "Office Floor Plan" },
                    { 3, 1, "application/pdf", new DateTime(2026, 4, 2, 11, 15, 0, 0, DateTimeKind.Utc), null, "quarterly-report-q1.pdf", 916L, "3f2b8c1e9a4d4c6b8e7f1a2b3c4d5e03.pdf", "Quarterly Report Q1" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentNotes_DocumentId",
                table: "DocumentNotes",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_CompanyId",
                table: "Documents",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DeletedDate",
                table: "Documents",
                column: "DeletedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Title",
                table: "Documents",
                column: "Title");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentNotes");

            migrationBuilder.DropTable(
                name: "Documents");
        }
    }
}
