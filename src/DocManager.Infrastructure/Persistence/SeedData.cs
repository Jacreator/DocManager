using DocManager.Domain.Entities;

namespace DocManager.Infrastructure.Persistence;

/// <summary>
/// Sample documents for company 1. The matching files ship with the web project
/// in App_Data/uploads so the seeded documents can be previewed.
/// </summary>
internal static class SeedData
{
    public const int SampleCompanyId = 1;

    public static readonly Document[] Documents =
    [
        new()
        {
            Id = 1,
            CompanyId = SampleCompanyId,
            Title = "Employee Handbook",
            FileName = "employee-handbook.pdf",
            StoredFileName = "3f2b8c1e9a4d4c6b8e7f1a2b3c4d5e01.pdf",
            ContentType = "application/pdf",
            FileSize = 1137,
            DateAdded = new DateTime(2026, 1, 5, 9, 0, 0, DateTimeKind.Utc)
        },
        new()
        {
            Id = 2,
            CompanyId = SampleCompanyId,
            Title = "Office Floor Plan",
            FileName = "office-floor-plan.png",
            StoredFileName = "3f2b8c1e9a4d4c6b8e7f1a2b3c4d5e02.png",
            ContentType = "image/png",
            FileSize = 3128,
            DateAdded = new DateTime(2026, 2, 10, 14, 30, 0, DateTimeKind.Utc)
        },
        new()
        {
            Id = 3,
            CompanyId = SampleCompanyId,
            Title = "Quarterly Report Q1",
            FileName = "quarterly-report-q1.pdf",
            StoredFileName = "3f2b8c1e9a4d4c6b8e7f1a2b3c4d5e03.pdf",
            ContentType = "application/pdf",
            FileSize = 916,
            DateAdded = new DateTime(2026, 4, 2, 11, 15, 0, DateTimeKind.Utc)
        }
    ];
}
