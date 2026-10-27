namespace DocManager.Application.Notes;

public sealed record NoteDto(int Id, string NoteText, decimal PositionTop, decimal PositionLeft, DateTime DateAdded);
