using CrisisManagement.Data;
using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Data.Repositories;
using CrisisManagement.Features.Users.ViewModels;
using CrisisManagement.Shared.Common;

namespace CrisisManagement.Features.Users.Services;

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

        var name = UploadChecks.RequirePdf(fileName, content, MaxFileBytes);

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
