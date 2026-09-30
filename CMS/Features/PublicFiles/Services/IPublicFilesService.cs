using CMS.Features.PublicFiles.ViewModels;

namespace CMS.Features.PublicFiles.Services;

public interface IPublicFilesService
{
    Task<List<HelpFileViewModel>> GetHelpFilesAsync(CancellationToken ct);

    // Null when there is no such active help document.
    Task<HelpFileContentViewModel?> GetHelpFileAsync(int id, CancellationToken ct);
}
