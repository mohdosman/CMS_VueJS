using CrisisManagement.Shared.Common.Files;

namespace CrisisManagement.Shared.Common;

// Checks for an uploaded file, shared by every screen that takes one. A failure is a validation error on "file"
// (the SPA shows it under the file input). The name returned is the bare file name, never a path.
public static class UploadChecks
{
    // The extension and the file's own bytes must both say PDF, so a renamed file is refused.
    public static string RequirePdf(string? fileName, byte[] content, int maxBytes) =>
        Require(fileName, content, maxBytes, "PDF", FileSignatures.Pdf);

    // The SafetyNet file check: the name, the size, and the content must agree with one of the permitted types, and nothing
    // that looks like an executable or a script gets through under any name. label is what the message calls the type.
    public static string Require(string? fileName, byte[] content, int maxBytes, string label, params FileSignature[] permitted)
    {
        var name = RequireBasics(fileName, content, maxBytes);
        var checker = FileSignatures.Allowing(permitted);
        if (checker.TryIdentify(name, content, out _)) return name;
        if (checker.Denies(name, content) is { } denied)
            throw ValidationFailedException.For("file", $"'{name}' is a forbidden file type ({denied.Name}) and cannot be uploaded.");
        throw ValidationFailedException.For("file", $"Only {label} files are accepted. [{name}]");
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
