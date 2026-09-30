using CMS.Data;
using CMS.Data.Models.Domain;
using CMS.Data.Repositories;
using CMS.Features.Users.ViewModels;
using CMS.Shared.Common;

namespace CMS.Features.Users.Services;

// End user agreements: PDFs stored in CMS_Document against a user (port of the Blazor UserDocumentService).
// Every call resolves the user through UserService first, so the same role/provider scope applies as on
// the user screen. A null id from that resolver means the user does not exist.
public sealed class UserDocumentService(IUnitOfWork uow, UserService users)
{
    private const int MaxFileBytes = 10 * 1024 * 1024;

    public async Task<List<UserDocumentItem>?> ListAsync(Guid userKey) =>
        await users.ResolveScopedUserIdAsync(userKey) is int id ? await uow.Documents.GetUserAgreementsAsync(id) : null;

    // Null = user not found. A bad file is a validation error on "file".
    public async Task<UserDocumentItem?> UploadAsync(Guid userKey, string fileName, byte[] content)
    {
        if (await users.ResolveScopedUserIdAsync(userKey) is not int id) return null;

        var name = Path.GetFileName(fileName ?? "");
        if (content.Length == 0) throw ValidationFailedException.For("file", "The file is empty.");
        if (content.Length > MaxFileBytes) throw ValidationFailedException.For("file", $"The file exceeds the {MaxFileBytes / (1024 * 1024)} MB limit.");
        // Extension and magic bytes must both say PDF, so a renamed file is refused.
        if (!name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) || !content.AsSpan().StartsWith("%PDF-"u8))
            throw ValidationFailedException.For("file", $"Only PDF files are accepted. [{name}]");

        var doc = new Document
        {
            UserId = id, DocumentTypeId = DocumentRepository.UserAgreementTypeId, FileName = name,
            FileContent = content, MIMEType = "application/pdf", IsActive = true
        };
        uow.Documents.Add(doc);
        await uow.SaveChangesAsync();
        return new UserDocumentItem(doc.DocumentId, doc.FileName, doc.CreatedOn, content.Length);
    }

    public async Task<Document?> DownloadAsync(Guid userKey, int documentId) =>
        await users.ResolveScopedUserIdAsync(userKey) is int id ? await uow.Documents.GetUserAgreementAsync(id, documentId) : null;

    // False = user or document not found.
    public async Task<bool> DeleteAsync(Guid userKey, int documentId) =>
        await users.ResolveScopedUserIdAsync(userKey) is int id && await uow.Documents.DeleteUserAgreementAsync(id, documentId) > 0;
}
