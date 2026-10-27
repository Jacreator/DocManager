namespace DocManager.Web.Models;

public sealed record PaginationViewModel(int Page, int TotalPages, string Action, string? Search = null)
{
    private const int PagesAroundCurrent = 2;

    public int FirstVisiblePage => Math.Max(1, Page - PagesAroundCurrent);

    public int LastVisiblePage => Math.Min(TotalPages, Page + PagesAroundCurrent);
}
