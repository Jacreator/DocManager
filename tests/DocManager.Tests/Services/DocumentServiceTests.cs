using DocManager.Application.Abstractions;
using DocManager.Application.Common;
using DocManager.Application.Documents;
using DocManager.Application.Options;
using DocManager.Domain.Common;
using DocManager.Domain.Entities;
using DocManager.Domain.Interfaces;
using DocManager.Tests.Support;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace DocManager.Tests.Services;

public sealed class DocumentServiceTests
{
    private const int CompanyId = 7;
    private const int PageSize = 10;
    private const long MaxFileSizeBytes = 1024 * 1024;

    private readonly Mock<IDocumentRepository> _documents = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IFileStorage> _fileStorage = new();
    private readonly DocumentService _service;

    public DocumentServiceTests()
    {
        var options = Microsoft.Extensions.Options.Options.Create(new DocumentOptions
        {
            MaxFileSizeBytes = MaxFileSizeBytes,
            AllowedExtensions = [".pdf", ".docx", ".png", ".jpg"],
            PageSize = PageSize
        });

        _service = new DocumentService(
            _documents.Object,
            _unitOfWork.Object,
            _fileStorage.Object,
            new TestCurrentCompany(CompanyId),
            new UploadDocumentRequestValidator(options),
            options,
            NullLogger<DocumentService>.Instance);
    }

    [Fact]
    public async Task UploadAsync_ValidRequest_StoresFileAndDocument()
    {
        Document? added = null;
        _documents.Setup(r => r.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Callback<Document, CancellationToken>((d, _) => added = d);

        var result = await _service.UploadAsync(Request("  Contract  ", "Contract.PDF"));

        Assert.True(result.IsSuccess);
        Assert.NotNull(added);
        Assert.Equal(CompanyId, added.CompanyId);
        Assert.Equal("Contract", added.Title);
        Assert.Equal("Contract.PDF", added.FileName);
        Assert.Equal("application/pdf", added.ContentType);
        Assert.Matches("^[0-9a-f]{32}\\.pdf$", added.StoredFileName);
        _fileStorage.Verify(s => s.SaveAsync(It.IsAny<Stream>(), added.StoredFileName, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("", "file.pdf", 10, "Title is required.")]
    [InlineData("Title", "", 0, "Please choose a file to upload.")]
    [InlineData("Title", "virus.exe", 10, "Only these file types are allowed: .pdf, .docx, .png, .jpg.")]
    [InlineData("Title", "big.pdf", MaxFileSizeBytes + 1, "The file must be 1 MB or smaller.")]
    [InlineData("Title", "empty.pdf", 0, "The file is empty.")]
    public async Task UploadAsync_InvalidRequest_ReturnsValidationErrorWithoutSaving(
        string title, string fileName, long length, string expectedError)
    {
        var result = await _service.UploadAsync(Request(title, fileName, length));

        Assert.Equal(ResultStatus.ValidationError, result.Status);
        Assert.Contains(expectedError, result.Errors);
        _fileStorage.VerifyNoOtherCalls();
        _unitOfWork.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UploadAsync_DatabaseSaveFails_DeletesStoredFileAndRethrows()
    {
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("db down"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UploadAsync(Request("Title", "file.pdf")));

        _fileStorage.Verify(s => s.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_PageBelowOne_RequestsFirstPageWithConfiguredSize()
    {
        _documents.Setup(r => r.SearchAsync("report", 1, PageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Document>([new Document { Id = 1, Title = "report" }], 1, PageSize, 1));

        var result = await _service.SearchAsync(" report ", page: 0);

        Assert.True(result.IsSuccess);
        Assert.Equal("report", Assert.Single(result.Value!.Items).Title);
    }

    [Fact]
    public async Task GetDetailsAsync_UnknownDocument_ReturnsNotFound()
    {
        var result = await _service.GetDetailsAsync(42);

        Assert.Equal(ResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task GetDetailsAsync_ExistingDocument_MapsDocumentAndNotes()
    {
        var document = new Document { Id = 5, Title = "Plan" };
        document.Notes.Add(new DocumentNote { Id = 9, NoteText = "Note", PositionTop = 12.5m, PositionLeft = 40m });
        _documents.Setup(r => r.GetWithNotesAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(document);

        var result = await _service.GetDetailsAsync(5);

        Assert.True(result.IsSuccess);
        Assert.Equal("Plan", result.Value!.Title);
        var note = Assert.Single(result.Value.Notes);
        Assert.Equal((9, 12.5m, 40m), (note.Id, note.PositionTop, note.PositionLeft));
    }

    [Fact]
    public async Task SoftDeleteAsync_ExistingDocument_SetsDeletedDateAndSaves()
    {
        var document = new Document { Id = 3 };
        _documents.Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(document);

        var result = await _service.SoftDeleteAsync(3);

        Assert.True(result.IsSuccess);
        Assert.NotNull(document.DeletedDate);
        Assert.Equal(DateTimeKind.Utc, document.DeletedDate.Value.Kind);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SoftDeleteAsync_UnknownDocument_ReturnsNotFoundWithoutSaving()
    {
        var result = await _service.SoftDeleteAsync(3);

        Assert.Equal(ResultStatus.NotFound, result.Status);
        _unitOfWork.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RestoreAsync_DeletedDocument_ClearsDeletedDate()
    {
        var document = new Document { Id = 4 };
        document.SoftDelete();
        _documents.Setup(r => r.GetDeletedByIdAsync(4, It.IsAny<CancellationToken>())).ReturnsAsync(document);

        var result = await _service.RestoreAsync(4);

        Assert.True(result.IsSuccess);
        Assert.Null(document.DeletedDate);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetFileAsync_FileMissingFromStorage_ReturnsNotFound()
    {
        _documents.Setup(r => r.GetByIdAsync(6, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Document { Id = 6, StoredFileName = "missing.pdf" });

        var result = await _service.GetFileAsync(6);

        Assert.Equal(ResultStatus.NotFound, result.Status);
    }

    private static UploadDocumentRequest Request(string title, string fileName, long length = 100) =>
        new(title, fileName, length, new MemoryStream(new byte[10]));
}
