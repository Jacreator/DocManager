namespace DocManager.Application.Notes;

public sealed record CreateNoteRequest(string NoteText, decimal PositionTop, decimal PositionLeft);
