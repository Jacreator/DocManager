using DocManager.Domain.Entities;

namespace DocManager.Infrastructure.Persistence;

/// <summary>
/// Sample data for two companies, so company isolation can be tried out. The matching files ship
/// with the web project in App_Data/uploads so the seeded documents can be previewed.
/// </summary>
/// <remarks>
/// Seed rows added after the initial migration use ids from <see cref="ReservedIdStart"/> upwards,
/// so they cannot collide with rows users have already created through the identity columns.
/// </remarks>
internal static class SeedData
{
    public const int SampleCompanyId = 1;
    public const int SecondCompanyId = 2;

    private const int ReservedIdStart = 1001;

    private const string Pdf = "application/pdf";
    private const string Png = "image/png";

    public static readonly Document[] Documents =
    [
        Sample(1, SampleCompanyId, "Employee Handbook", "employee-handbook.pdf", "e01.pdf", Pdf, 1137, new DateTime(2026, 1, 5, 9, 0, 0, DateTimeKind.Utc)),
        Sample(2, SampleCompanyId, "Office Floor Plan", "office-floor-plan.png", "e02.png", Png, 3128, new DateTime(2026, 2, 10, 14, 30, 0, DateTimeKind.Utc)),
        Sample(3, SampleCompanyId, "Quarterly Report Q1", "quarterly-report-q1.pdf", "e03.pdf", Pdf, 916, new DateTime(2026, 4, 2, 11, 15, 0, DateTimeKind.Utc)),

        Sample(ReservedIdStart, SecondCompanyId, "Supplier Contract", "supplier-contract.pdf", "e04.pdf", Pdf, 960, new DateTime(2026, 3, 1, 8, 0, 0, DateTimeKind.Utc)),
        Sample(ReservedIdStart + 1, SecondCompanyId, "Warehouse Layout", "warehouse-layout.png", "e05.png", Png, 2521, new DateTime(2026, 3, 18, 13, 45, 0, DateTimeKind.Utc)),
        Sample(ReservedIdStart + 2, SecondCompanyId, "Board Meeting Minutes", "board-meeting-minutes.pdf", "e06.pdf", Pdf, 960, new DateTime(2026, 5, 20, 16, 0, 0, DateTimeKind.Utc))
    ];

    /// <summary>
    /// Already in company 2's recycle bin. Seeded as an anonymous object because
    /// <see cref="Document.DeletedDate"/> can only be set through <see cref="Document.SoftDelete"/>.
    /// </summary>
    public static readonly object[] DeletedDocuments =
    [
        new
        {
            Id = ReservedIdStart + 3,
            CompanyId = SecondCompanyId,
            Title = "Price List 2025",
            FileName = "price-list-2025.pdf",
            StoredFileName = StoredFileName("e07.pdf"),
            ContentType = Pdf,
            FileSize = 821L,
            DateAdded = new DateTime(2025, 11, 3, 10, 0, 0, DateTimeKind.Utc),
            DeletedDate = (DateTime?)new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc)
        }
    ];

    public static readonly DocumentNote[] Notes =
    [
        new()
        {
            Id = ReservedIdStart,
            DocumentId = ReservedIdStart,
            NoteText = "Company 2 only: renegotiate payment terms before renewal.",
            PositionTop = 30m,
            PositionLeft = 55m,
            DateAdded = new DateTime(2026, 3, 2, 9, 30, 0, DateTimeKind.Utc)
        }
    ];

    private static Document Sample(
        int id, int companyId, string title, string fileName, string storedSuffix,
        string contentType, long fileSize, DateTime dateAdded) =>
        new()
        {
            Id = id,
            CompanyId = companyId,
            Title = title,
            FileName = fileName,
            StoredFileName = StoredFileName(storedSuffix),
            ContentType = contentType,
            FileSize = fileSize,
            DateAdded = dateAdded
        };

    private static string StoredFileName(string suffix) => $"3f2b8c1e9a4d4c6b8e7f1a2b3c4d5{suffix}";
}
