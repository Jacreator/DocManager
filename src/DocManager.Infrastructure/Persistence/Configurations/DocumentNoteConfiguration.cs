using DocManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocManager.Infrastructure.Persistence.Configurations;

internal sealed class DocumentNoteConfiguration : IEntityTypeConfiguration<DocumentNote>
{
    public void Configure(EntityTypeBuilder<DocumentNote> builder)
    {
        builder.ToTable("DocumentNotes");
        builder.HasKey(n => n.Id);

        builder.Property(n => n.NoteText).HasMaxLength(1000).IsRequired();
        builder.Property(n => n.PositionTop).HasPrecision(5, 2);
        builder.Property(n => n.PositionLeft).HasPrecision(5, 2);
        builder.Property(n => n.DateAdded).IsRequired();

        builder.HasIndex(n => n.DocumentId);

        builder.HasData(SeedData.Notes);
    }
}
