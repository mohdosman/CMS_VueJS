namespace CMS.Features.PublicFiles.ViewModels;

// One row of the Help dialog.
public sealed record HelpFileViewModel(int Id, string FileName, DateTime CreatedOn, int FileSize);

// A help document to stream back to the browser.
public sealed record HelpFileContentViewModel(string FileName, string MimeType, byte[] Content);
