using DocManager.Domain.Entities;
using DocManager.Infrastructure.Persistence.Repositories;
using DocManager.Tests.Support;

namespace DocManager.Tests.Repositories;

public sealed class DocumentRepositoryTests : IDisposable
{
    private const int PageSize = 10;
    private readonly SqliteTestDatabase _database = new();

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task SearchAsync_WithTitle_ReturnsOnlyMatchingDocuments()
    {
        await _database.AddDocumentAsync("Annual Report");
        await _database.AddDocumentAsync("Holiday Policy");
        await _database.AddDocumentAsync("Report Template");

        var result = await CreateRepository().SearchAsync("Report", 1, PageSize, CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, d => Assert.Contains("Report", d.Title));
    }

    [Fact]
    public async Task SearchAsync_PagesResultsNewestFirst()
    {
        for (var i = 1; i <= 5; i++)
        {
            await _database.AddDocumentAsync($"Document {i}");
        }

        var result = await CreateRepository().SearchAsync(null, page: 2, pageSize: 2, CancellationToken.None);

        Assert.Equal(5, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(["Document 3", "Document 2"], result.Items.Select(d => d.Title));
        Assert.True(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
    }

    [Fact]
    public async Task SearchAsync_ExcludesSoftDeletedDocuments()
    {
        await _database.AddDocumentAsync("Visible");
        await _database.AddDocumentAsync("Deleted", deleted: true);

        var result = await CreateRepository().SearchAsync(null, 1, PageSize, CancellationToken.None);

        Assert.Equal("Visible", Assert.Single(result.Items).Title);
    }

    [Fact]
    public async Task SearchAsync_ExcludesOtherCompaniesDocuments()
    {
        await _database.AddDocumentAsync("Ours");
        await _database.AddDocumentAsync("Theirs", SqliteTestDatabase.OtherCompanyId);

        var result = await CreateRepository().SearchAsync(null, 1, PageSize, CancellationToken.None);

        Assert.Equal("Ours", Assert.Single(result.Items).Title);
    }

    [Fact]
    public async Task GetByIdAsync_DocumentOfOtherCompany_ReturnsNull()
    {
        var theirs = await _database.AddDocumentAsync("Theirs", SqliteTestDatabase.OtherCompanyId);

        var document = await CreateRepository().GetByIdAsync(theirs.Id);

        Assert.Null(document);
    }

    [Fact]
    public async Task GetDeletedAsync_ReturnsOnlyDeletedDocumentsOfCurrentCompany()
    {
        await _database.AddDocumentAsync("Active");
        await _database.AddDocumentAsync("Deleted", deleted: true);
        await _database.AddDocumentAsync("Theirs deleted", SqliteTestDatabase.OtherCompanyId, deleted: true);

        var result = await CreateRepository().GetDeletedAsync(1, PageSize, CancellationToken.None);

        var document = Assert.Single(result.Items);
        Assert.Equal("Deleted", document.Title);
        Assert.NotNull(document.DeletedDate);
    }

    [Fact]
    public async Task GetDeletedByIdAsync_DeletedDocumentOfOtherCompany_ReturnsNull()
    {
        var theirs = await _database.AddDocumentAsync("Theirs", SqliteTestDatabase.OtherCompanyId, deleted: true);

        var document = await CreateRepository().GetDeletedByIdAsync(theirs.Id, CancellationToken.None);

        Assert.Null(document);
    }

    [Fact]
    public async Task GetWithNotesAsync_IncludesNotes()
    {
        var document = await _database.AddDocumentAsync("With notes");
        await using (var context = _database.CreateContext())
        {
            context.DocumentNotes.Add(new DocumentNote { DocumentId = document.Id, NoteText = "Check this", PositionTop = 10, PositionLeft = 20 });
            await context.SaveChangesAsync();
        }

        var loaded = await CreateRepository().GetWithNotesAsync(document.Id, CancellationToken.None);

        Assert.NotNull(loaded);
        Assert.Equal("Check this", Assert.Single(loaded.Notes).NoteText);
    }

    [Fact]
    public async Task SaveChangesAsync_SetsDateAddedInUtc()
    {
        var before = DateTime.UtcNow;

        var document = await _database.AddDocumentAsync("Stamped");

        Assert.Equal(DateTimeKind.Utc, document.DateAdded.Kind);
        Assert.InRange(document.DateAdded, before, DateTime.UtcNow);
    }

    private DocumentRepository CreateRepository() => new(_database.CreateContext());
}
