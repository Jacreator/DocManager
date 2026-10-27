using DocManager.Application.Common;
using DocManager.Domain.Common;

namespace DocManager.Application.Documents;

public interface IDocumentService
{
    /// <summary>Validates and stores a new document. Returns the new document id.</summary>
    Task<Result<int>> UploadAsync(UploadDocumentRequest request, CancellationToken ct = default);

    Task<Result<PagedResult<DocumentSummaryDto>>> SearchAsync(string? title, int page, CancellationToken ct = default);

    Task<Result<DocumentDetailsDto>> GetDetailsAsync(int id, CancellationToken ct = default);

    Task<Result> SoftDeleteAsync(int id, CancellationToken ct = default);

    Task<Result<PagedResult<DocumentSummaryDto>>> GetDeletedAsync(int page, CancellationToken ct = default);

    Task<Result> RestoreAsync(int id, CancellationToken ct = default);

    Task<Result<FileDownloadDto>> GetFileAsync(int id, CancellationToken ct = default);
}
