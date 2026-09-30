using CMS.Data.Models.Domain;
using CMS.Data.Repositories.Interfaces;
using CMS.Features.PublicFiles.ViewModels;

namespace CMS.Data.Repositories.Interfaces;

public interface IDocumentRepository : IRepository<Document>
{
    // Help files: listing projects the size in SQL, so the document bytes are never loaded.
    Task<List<HelpFileViewModel>> GetHelpFilesAsync(CancellationToken ct = default);
    // Only an active help document; per-user agreements are never returned.
    Task<Document?> GetHelpFileAsync(int id, CancellationToken ct = default);
    Task<bool> AnyForUserAsync(int userId);
}
