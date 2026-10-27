using DocManager.Domain.Common;
using DocManager.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DocManager.Infrastructure.Persistence.Repositories;

public abstract class Repository<T>(AppDbContext context) : IRepository<T> where T : BaseEntity
{
    protected AppDbContext Context { get; } = context;

    protected DbSet<T> Set => Context.Set<T>();

    public Task<T?> GetByIdAsync(int id, CancellationToken ct = default) =>
        Set.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task AddAsync(T entity, CancellationToken ct = default) =>
        await Set.AddAsync(entity, ct);

    public void Update(T entity) => Set.Update(entity);

    protected static async Task<PagedResult<T>> ToPagedResultAsync(
        IQueryable<T> query, int page, int pageSize, CancellationToken ct)
    {
        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<T>(items, page, pageSize, totalCount);
    }
}
