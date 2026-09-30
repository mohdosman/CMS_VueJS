using CMS.Data;
using CMS.Features.PublicFiles.Mapping;
using CMS.Features.PublicFiles.ViewModels;

namespace CMS.Features.PublicFiles.Services;

public sealed class PublicFilesService(IUnitOfWork unitOfWork) : IPublicFilesService
{
    public Task<List<HelpFileViewModel>> GetHelpFilesAsync(CancellationToken ct) =>
        unitOfWork.Documents.GetHelpFilesAsync(ct);

    public async Task<HelpFileContentViewModel?> GetHelpFileAsync(int id, CancellationToken ct) =>
        (await unitOfWork.Documents.GetHelpFileAsync(id, ct))?.ToContentViewModel();
}
