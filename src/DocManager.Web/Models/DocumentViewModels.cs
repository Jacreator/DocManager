using DocManager.Application.Documents;
using DocManager.Domain.Common;

namespace DocManager.Web.Models;

public sealed record DocumentIndexViewModel(string? Search, PagedResult<DocumentSummaryDto> Documents);

public sealed record RecycleBinViewModel(PagedResult<DocumentSummaryDto> Documents);

public enum PreviewKind
{
    None,
    Pdf,
    Image
}

public sealed record DocumentDetailsViewModel(DocumentDetailsDto Document)
{
    public PreviewKind PreviewKind => Document.ContentType switch
    {
        "application/pdf" => PreviewKind.Pdf,
        _ when Document.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase) => PreviewKind.Image,
        _ => PreviewKind.None
    };
}
