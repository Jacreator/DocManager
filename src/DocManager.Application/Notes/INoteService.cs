using DocManager.Application.Common;

namespace DocManager.Application.Notes;

public interface INoteService
{
    Task<Result<NoteDto>> AddNoteAsync(int documentId, CreateNoteRequest request, CancellationToken ct = default);

    Task<Result<IReadOnlyList<NoteDto>>> GetNotesAsync(int documentId, CancellationToken ct = default);
}
