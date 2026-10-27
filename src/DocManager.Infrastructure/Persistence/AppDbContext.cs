using DocManager.Application.Abstractions;
using DocManager.Domain.Common;
using DocManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocManager.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options, ICurrentCompany currentCompany)
    : DbContext(options)
{
    public DbSet<Document> Documents => Set<Document>();

    public DbSet<DocumentNote> DocumentNotes => Set<DocumentNote>();

    /// <summary>Referenced by the global query filters; EF re-evaluates it for every query.</summary>
    public int CurrentCompanyId => currentCompany.CompanyId;

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        StampDateAdded();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.Entity<Document>()
            .HasQueryFilter(d => d.DeletedDate == null && d.CompanyId == CurrentCompanyId);

        // Mirrors the Document filter so notes of hidden documents are hidden too.
        modelBuilder.Entity<DocumentNote>()
            .HasQueryFilter(n => n.Document!.DeletedDate == null && n.Document.CompanyId == CurrentCompanyId);
    }

    private void StampDateAdded()
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<BaseEntity>().Where(e => e.State == EntityState.Added))
        {
            entry.Entity.DateAdded = now;
        }
    }
}
