namespace CrisisManagement.Shared.Common;

// A page of search results plus the total across all pages; every paged search endpoint returns this shape.
public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
}

// Id + label (+ optional short text, e.g. an abbreviation) for a lookup list.
public sealed record LookupItem(int Id, string Label, string? Short = null);
