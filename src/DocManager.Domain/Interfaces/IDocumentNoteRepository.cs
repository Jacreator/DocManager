using DocManager.Domain.Entities;

namespace DocManager.Domain.Interfaces;

public interface IDocumentNoteRepository : IRepository<DocumentNote>
{
    Task<IReadOnlyList<DocumentNote>> GetByDocumentAsync(int documentId, CancellationToken ct);
}
