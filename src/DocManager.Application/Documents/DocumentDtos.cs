using DocManager.Application.Notes;

namespace DocManager.Application.Documents;

public sealed record DocumentSummaryDto(
    int Id,
    string Title,
    string FileName,
    string ContentType,
    long FileSize,
    DateTime DateAdded,
    DateTime? DeletedDate);

public sealed record DocumentDetailsDto(
    int Id,
    string Title,
    string FileName,
    string ContentType,
    long FileSize,
    DateTime DateAdded,
    IReadOnlyList<NoteDto> Notes);

public sealed record FileDownloadDto(Stream Content, string ContentType, string FileName);
