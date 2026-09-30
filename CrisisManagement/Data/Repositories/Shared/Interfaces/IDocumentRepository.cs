using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Data.Repositories.Interfaces;
using CrisisManagement.Features.PublicFiles.ViewModels;
using CrisisManagement.Features.Users.ViewModels;
using CrisisManagement.Shared.Common;

namespace CrisisManagement.Data.Repositories.Interfaces;

public interface IDocumentRepository : IRepository<Document>
{
    // Help files: listing projects the size in SQL, so the document bytes are never loaded.
    Task<List<HelpFileViewModel>> GetHelpFilesAsync(CancellationToken ct = default);
    // Only an active help document; per-user agreements are never returned.
    Task<Document?> GetHelpFileAsync(int id, CancellationToken ct = default);
    Task<bool> AnyForUserAsync(int userId);

    // End user agreements (document type 3) held against a user. Listing never loads the bytes.
    Task<List<UserDocumentItem>> GetUserAgreementsAsync(int userId);
    Task<Document?> GetUserAgreementAsync(int userId, int documentId);
    // Direct delete (no bytes loaded); returns rows removed.
    Task<int> DeleteUserAgreementAsync(int userId, int documentId);

    // Public files screen: the active help documents (type 2, owned by no user), one page.
    // sortBy is "id", "filename", "filesize" or "createdon" (default).
    Task<PagedResult<HelpFileViewModel>> SearchPublicFilesAsync(string? fileName, string? sortBy, bool desc, int page, int size);
    // Direct delete; only ever a public file, never a user's agreement. Returns rows removed.
    Task<int> DeletePublicFileAsync(int id);
}
