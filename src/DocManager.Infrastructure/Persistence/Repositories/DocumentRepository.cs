using DocManager.Domain.Common;
using DocManager.Domain.Entities;
using DocManager.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DocManager.Infrastructure.Persistence.Repositories;

public sealed class DocumentRepository(AppDbContext context) : Repository<Document>(context), IDocumentRepository
{
    public Task<PagedResult<Document>> SearchAsync(string? title, int page, int pageSize, CancellationToken ct)
    {
        var query = Set.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(title))
        {
            query = query.Where(d => d.Title.Contains(title));
        }

        var ordered = query
            .OrderByDescending(d => d.DateAdded)
            .ThenByDescending(d => d.Id);

        return ToPagedResultAsync(ordered, page, pageSize, ct);
    }

    public Task<Document?> GetWithNotesAsync(int id, CancellationToken ct) =>
        Set.AsNoTracking()
            .Include(d => d.Notes)
            .FirstOrDefaultAsync(d => d.Id == id, ct);

    public Task<PagedResult<Document>> GetDeletedAsync(int page, int pageSize, CancellationToken ct)
    {
        var ordered = DeletedForCurrentCompany()
            .AsNoTracking()
            .OrderByDescending(d => d.DeletedDate)
            .ThenByDescending(d => d.Id);

        return ToPagedResultAsync(ordered, page, pageSize, ct);
    }

    public Task<Document?> GetDeletedByIdAsync(int id, CancellationToken ct) =>
        DeletedForCurrentCompany().FirstOrDefaultAsync(d => d.Id == id, ct);

    /// <summary>
    /// IgnoreQueryFilters drops every filter on Document, so the company filter is re-applied here.
    /// </summary>
    private IQueryable<Document> DeletedForCurrentCompany()
    {
        var companyId = Context.CurrentCompanyId;
        return Set.IgnoreQueryFilters()
            .Where(d => d.CompanyId == companyId && d.DeletedDate != null);
    }
}
