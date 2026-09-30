namespace CrisisManagement.Features.Reports.ViewModels;

public sealed class ReportSearchRequest
{
    public string? ReportName { get; set; }
    public string? Description { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public bool SortDesc { get; set; }
}

public sealed record ReportListItem(int Id, Guid ReportKey, string ReportName, string FileName, string Description, string ExportOption, DateTime UpdatedOn);

// One shape for the editor. On an existing report only the description and the export option change.
public sealed class ReportEditModel
{
    public int? Id { get; set; }
    public string? RowVersion { get; set; }
    public Guid? ReportKey { get; set; }
    public string? ReportName { get; set; }
    public string? FileName { get; set; }
    public string? Description { get; set; }
    public string? ExportOption { get; set; }
    public DateTime? CreatedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }
}

// Where the browser window goes to open the report: a page that posts the signed token to the report server.
public sealed record ReportRun(string RedirectUrl, DateTime ExpiresAt);
