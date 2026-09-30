using CMS.Data.Models.Domain;
using CMS.Data.Repositories;
using CMS.Features.PublicFiles.ViewModels;

namespace CMS.Features.PublicFiles.Repositories;

public interface IPublicFilesRepository : IRepository<Document>
{
    // Listing projects the size in SQL, so the document bytes are never loaded for the Help dialog.
    Task<List<HelpFileViewModel>> GetHelpFilesAsync(CancellationToken ct = default);

    // Only an active help document; per-user agreements are never returned.
    Task<Document?> GetHelpFileAsync(int id, CancellationToken ct = default);
}
