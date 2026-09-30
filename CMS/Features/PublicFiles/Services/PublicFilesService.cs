using CMS.Data;
using CMS.Features.PublicFiles.Mapping;
using CMS.Features.PublicFiles.ViewModels;

namespace CMS.Features.PublicFiles.Services;

public sealed class PublicFilesService(IUnitOfWork unitOfWork) : IPublicFilesService
{
    public Task<List<HelpFileViewModel>> GetHelpFilesAsync(CancellationToken ct) =>
        unitOfWork.PublicFiles.GetHelpFilesAsync(ct);

    public async Task<HelpFileContentViewModel?> GetHelpFileAsync(int id, CancellationToken ct) =>
        (await unitOfWork.PublicFiles.GetHelpFileAsync(id, ct))?.ToContentViewModel();
}
