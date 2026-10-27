using DocManager.Domain.Entities;

namespace DocManager.Application.Notes;

internal static class NoteMappings
{
    private const int PositionDecimals = 2;

    public static NoteDto ToDto(this DocumentNote note) =>
        new(note.Id, note.NoteText, note.PositionTop, note.PositionLeft, note.DateAdded);

    public static DocumentNote ToEntity(this CreateNoteRequest request, int documentId) =>
        new()
        {
            DocumentId = documentId,
            NoteText = request.NoteText.Trim(),
            PositionTop = Math.Round(request.PositionTop, PositionDecimals),
            PositionLeft = Math.Round(request.PositionLeft, PositionDecimals)
        };
}
