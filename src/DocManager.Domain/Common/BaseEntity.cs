namespace DocManager.Domain.Common;

public abstract class BaseEntity
{
    public int Id { get; set; }

    /// <summary>UTC timestamp, set automatically when the entity is first saved.</summary>
    public DateTime DateAdded { get; set; }
}
