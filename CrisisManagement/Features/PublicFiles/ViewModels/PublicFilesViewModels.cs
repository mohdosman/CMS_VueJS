namespace CrisisManagement.Features.PublicFiles.ViewModels;

// One row of the Help dialog and of the Public Files screen.
public sealed record HelpFileViewModel(int Id, string FileName, DateTime CreatedOn, int FileSize);

// A help document to stream back to the browser.
public sealed record HelpFileContentViewModel(string FileName, string MimeType, byte[] Content);

public sealed class PublicFileSearchRequest
{
    public string? FileName { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public bool SortDesc { get; set; }
}
