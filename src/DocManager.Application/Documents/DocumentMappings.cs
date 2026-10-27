using DocManager.Application.Notes;
using DocManager.Domain.Entities;

namespace DocManager.Application.Documents;

internal static class DocumentMappings
{
    public static DocumentSummaryDto ToSummaryDto(this Document document) =>
        new(document.Id,
            document.Title,
            document.FileName,
            document.ContentType,
            document.FileSize,
            document.DateAdded,
            document.DeletedDate);

    public static DocumentDetailsDto ToDetailsDto(this Document document) =>
        new(document.Id,
            document.Title,
            document.FileName,
            document.ContentType,
            document.FileSize,
            document.DateAdded,
            document.Notes.OrderBy(n => n.DateAdded).Select(n => n.ToDto()).ToList());
}
