using DocManager.Application.Common;
using DocManager.Domain.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DocManager.Application.Notes;

public sealed class NoteService(
    IDocumentRepository documents,
    IDocumentNoteRepository notes,
    IUnitOfWork unitOfWork,
    IValidator<CreateNoteRequest> validator,
    ILogger<NoteService> logger) : INoteService
{
    public async Task<Result<NoteDto>> AddNoteAsync(int documentId, CreateNoteRequest request, CancellationToken ct = default)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Result<NoteDto>.ValidationError(validation.ErrorMessages());
        }

        if (await documents.GetByIdAsync(documentId, ct) is null)
        {
            return Result<NoteDto>.NotFound(Errors.DocumentNotFound(documentId));
        }

        var note = request.ToEntity(documentId);
        await notes.AddAsync(note, ct);
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Added note {NoteId} to document {DocumentId}", note.Id, documentId);
        return Result<NoteDto>.Success(note.ToDto());
    }

    public async Task<Result<IReadOnlyList<NoteDto>>> GetNotesAsync(int documentId, CancellationToken ct = default)
    {
        if (await documents.GetByIdAsync(documentId, ct) is null)
        {
            return Result<IReadOnlyList<NoteDto>>.NotFound(Errors.DocumentNotFound(documentId));
        }

        var documentNotes = await notes.GetByDocumentAsync(documentId, ct);
        return Result<IReadOnlyList<NoteDto>>.Success(documentNotes.Select(n => n.ToDto()).ToList());
    }
}
