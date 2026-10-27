using DocManager.Domain.Common;
using DocManager.Domain.Entities;

namespace DocManager.Domain.Interfaces;

public interface IDocumentRepository : IRepository<Document>
{
    Task<PagedResult<Document>> SearchAsync(string? title, int page, int pageSize, CancellationToken ct);

    Task<Document?> GetWithNotesAsync(int id, CancellationToken ct);

    Task<PagedResult<Document>> GetDeletedAsync(int page, int pageSize, CancellationToken ct);

    Task<Document?> GetDeletedByIdAsync(int id, CancellationToken ct);
}
