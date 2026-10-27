using DocManager.Domain.Common;

namespace DocManager.Domain.Entities;

public class DocumentNote : BaseEntity
{
    public int DocumentId { get; set; }

    public Document? Document { get; set; }

    public string NoteText { get; set; } = string.Empty;

    /// <summary>Vertical position as a percentage (0-100) of the document viewer height.</summary>
    public decimal PositionTop { get; set; }

    /// <summary>Horizontal position as a percentage (0-100) of the document viewer width.</summary>
    public decimal PositionLeft { get; set; }
}
