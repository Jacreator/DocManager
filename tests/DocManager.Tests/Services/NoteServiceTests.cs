using DocManager.Application.Common;
using DocManager.Application.Notes;
using DocManager.Domain.Entities;
using DocManager.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace DocManager.Tests.Services;

public sealed class NoteServiceTests
{
    private const int DocumentId = 1;

    private readonly Mock<IDocumentRepository> _documents = new();
    private readonly Mock<IDocumentNoteRepository> _notes = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly NoteService _service;

    public NoteServiceTests()
    {
        _service = new NoteService(
            _documents.Object,
            _notes.Object,
            _unitOfWork.Object,
            new CreateNoteRequestValidator(),
            NullLogger<NoteService>.Instance);
    }

    [Fact]
    public async Task AddNoteAsync_ValidRequest_AddsNoteAndReturnsDto()
    {
        DocumentExists();
        DocumentNote? added = null;
        _notes.Setup(r => r.AddAsync(It.IsAny<DocumentNote>(), It.IsAny<CancellationToken>()))
            .Callback<DocumentNote, CancellationToken>((n, _) => added = n);

        var result = await _service.AddNoteAsync(DocumentId, new CreateNoteRequest("  Sign here  ", 25.456m, 50m));

        Assert.True(result.IsSuccess);
        Assert.NotNull(added);
        Assert.Equal(DocumentId, added.DocumentId);
        Assert.Equal("Sign here", result.Value!.NoteText);
        Assert.Equal(25.46m, result.Value.PositionTop);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddNoteAsync_UnknownDocument_ReturnsNotFoundWithoutSaving()
    {
        var result = await _service.AddNoteAsync(DocumentId, new CreateNoteRequest("Text", 10, 10));

        Assert.Equal(ResultStatus.NotFound, result.Status);
        _notes.VerifyNoOtherCalls();
        _unitOfWork.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task AddNoteAsync_InvalidRequest_ReturnsValidationErrors()
    {
        var result = await _service.AddNoteAsync(DocumentId, new CreateNoteRequest("", 150, -1));

        Assert.Equal(ResultStatus.ValidationError, result.Status);
        Assert.Equal(3, result.Errors.Count);
        _documents.VerifyNoOtherCalls();
        _unitOfWork.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task AddNoteAsync_TextTooLong_ReturnsValidationError()
    {
        var text = new string('x', CreateNoteRequestValidator.NoteTextMaxLength + 1);

        var result = await _service.AddNoteAsync(DocumentId, new CreateNoteRequest(text, 10, 10));

        Assert.Equal(ResultStatus.ValidationError, result.Status);
    }

    [Fact]
    public async Task GetNotesAsync_UnknownDocument_ReturnsNotFound()
    {
        var result = await _service.GetNotesAsync(DocumentId);

        Assert.Equal(ResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task GetNotesAsync_ExistingDocument_ReturnsNotes()
    {
        DocumentExists();
        _notes.Setup(r => r.GetByDocumentAsync(DocumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new DocumentNote { Id = 1, NoteText = "A" }, new DocumentNote { Id = 2, NoteText = "B" }]);

        var result = await _service.GetNotesAsync(DocumentId);

        Assert.Equal(["A", "B"], result.Value!.Select(n => n.NoteText));
    }

    private void DocumentExists() =>
        _documents.Setup(r => r.GetByIdAsync(DocumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Document { Id = DocumentId });
}
