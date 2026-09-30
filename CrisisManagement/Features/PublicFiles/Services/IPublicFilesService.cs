using CrisisManagement.Features.PublicFiles.ViewModels;
using CrisisManagement.Shared.Common;

namespace CrisisManagement.Features.PublicFiles.Services;

public interface IPublicFilesService
{
    // Help dialog (any signed-in user).
    Task<List<HelpFileViewModel>> GetHelpFilesAsync(CancellationToken ct);
    // Null when there is no such active help document.
    Task<HelpFileContentViewModel?> GetHelpFileAsync(int id, CancellationToken ct);

    // Public Files screen (publicfiles.view / publicfiles.edit).
    Task<PagedResult<HelpFileViewModel>> SearchAsync(PublicFileSearchRequest request);
    Task<HelpFileViewModel> UploadAsync(string fileName, byte[] content);
    // False = no such public file.
    Task<bool> DeleteAsync(int id);
}
