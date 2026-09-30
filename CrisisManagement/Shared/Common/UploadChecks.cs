namespace CrisisManagement.Shared.Common;

// Checks for an uploaded file, shared by every screen that takes one. A failure is a validation error on "file"
// (the SPA shows it under the file input). The name returned is the bare file name, never a path.
public static class UploadChecks
{
    // The extension and the file's own first bytes must both say PDF, so a renamed file is refused.
    public static string RequirePdf(string? fileName, byte[] content, int maxBytes)
    {
        var name = RequireBasics(fileName, content, maxBytes);
        if (!name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) || !content.AsSpan().StartsWith("%PDF-"u8))
            throw ValidationFailedException.For("file", $"Only PDF files are accepted. [{name}]");
        return name;
    }

    // Not empty, not over the limit, has a name.
    public static string RequireBasics(string? fileName, byte[] content, int maxBytes)
    {
        var name = Path.GetFileName(fileName ?? "");
        if (name.Length == 0) throw ValidationFailedException.For("file", "The file has no name.");
        if (content.Length == 0) throw ValidationFailedException.For("file", "The file is empty.");
        if (content.Length > maxBytes) throw ValidationFailedException.For("file", $"The file exceeds the {maxBytes / (1024 * 1024)} MB limit.");
        return name;
    }
}
