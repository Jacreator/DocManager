using DocManager.Domain.Common;

namespace DocManager.Domain.Entities;

public class Document : BaseEntity
{
    public int CompanyId { get; set; }

    public string Title { get; set; } = string.Empty;

    /// <summary>Original file name as uploaded by the user.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>GUID-based name used on the storage medium.</summary>
    public string StoredFileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public DateTime? DeletedDate { get; private set; }

    public ICollection<DocumentNote> Notes { get; set; } = new List<DocumentNote>();

    public bool IsDeleted => DeletedDate is not null;

    public void SoftDelete() => DeletedDate ??= DateTime.UtcNow;

    public void Restore() => DeletedDate = null;
}
