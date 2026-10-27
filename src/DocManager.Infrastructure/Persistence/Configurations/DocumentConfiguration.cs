using DocManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocManager.Infrastructure.Persistence.Configurations;

internal sealed class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Title).HasMaxLength(200).IsRequired();
        builder.Property(d => d.FileName).HasMaxLength(255).IsRequired();
        builder.Property(d => d.StoredFileName).HasMaxLength(255).IsRequired();
        builder.Property(d => d.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(d => d.DateAdded).IsRequired();
        builder.Ignore(d => d.IsDeleted);

        builder.HasIndex(d => d.CompanyId);
        builder.HasIndex(d => d.Title);
        builder.HasIndex(d => d.DeletedDate);

        builder.HasMany(d => d.Notes)
            .WithOne(n => n.Document)
            .HasForeignKey(n => n.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(SeedData.Documents);
        builder.HasData(SeedData.DeletedDocuments);
    }
}
