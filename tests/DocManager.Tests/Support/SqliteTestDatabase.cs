using DocManager.Domain.Entities;
using DocManager.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DocManager.Tests.Support;

/// <summary>
/// An in-memory SQLite database that lives as long as this object. Contexts created from it
/// share the connection, so data written by one is visible to the next.
/// </summary>
public sealed class SqliteTestDatabase : IDisposable
{
    public const int CompanyId = 1;
    public const int OtherCompanyId = 2;

    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public SqliteTestDatabase()
    {
        _connection.Open();

        using var context = CreateContext();
        context.Database.EnsureCreated();

        // Start from an empty table rather than the sample seed data.
        context.Documents.IgnoreQueryFilters().ExecuteDelete();
    }

    public AppDbContext CreateContext(int companyId = CompanyId) =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options,
            new TestCurrentCompany(companyId));

    public async Task<Document> AddDocumentAsync(string title, int companyId = CompanyId, bool deleted = false)
    {
        var document = new Document
        {
            CompanyId = companyId,
            Title = title,
            FileName = $"{title}.pdf",
            StoredFileName = $"{Guid.NewGuid():N}.pdf",
            ContentType = "application/pdf",
            FileSize = 100
        };

        if (deleted)
        {
            document.SoftDelete();
        }

        await using var context = CreateContext(companyId);
        context.Documents.Add(document);
        await context.SaveChangesAsync();
        return document;
    }

    public void Dispose() => _connection.Dispose();
}
