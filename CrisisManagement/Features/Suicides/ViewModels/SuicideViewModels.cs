namespace CrisisManagement.Features.Suicides.ViewModels;

public sealed class SuicideFileSearchRequest
{
    public string? FileName { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public bool SortDesc { get; set; }
}

public sealed record SuicideFileListItem(int Id, string? FileName, int? FileSize, int? RecordCount, DateTime? CreatedOn);

public sealed class SuicideImportSearchRequest
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public bool SortDesc { get; set; }
}

// One death record of an uploaded file. Dates are shown as month/day/year with the blank parts left out.
public sealed record SuicideImportItem(
    int Id, string? LastName, string? FirstName, string? Sex, string? Ssn, string? DateOfBirth, string? DateOfDeath,
    string? DeathStateCountry, string? ResCounty, string? ResStateCountry, string? DeathManner, string? UsArmedForces, string? Provider);

public sealed record SuicideFileUploaded(int Id, string FileName, int RecordCount);

public sealed record SuicideFileDownload(string FileName, byte[] Content);
