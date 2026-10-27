using DocManager.Domain.Entities;
using DocManager.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DocManager.Infrastructure.Persistence.Repositories;

public sealed class DocumentNoteRepository(AppDbContext context)
    : Repository<DocumentNote>(context), IDocumentNoteRepository
{
    public async Task<IReadOnlyList<DocumentNote>> GetByDocumentAsync(int documentId, CancellationToken ct) =>
        await Set.AsNoTracking()
            .Where(n => n.DocumentId == documentId)
            .OrderBy(n => n.DateAdded)
            .ToListAsync(ct);
}
