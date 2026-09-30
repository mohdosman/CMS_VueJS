using CrisisManagement.Data;
using CrisisManagement.Features.PublicFiles.Mapping;
using CrisisManagement.Features.PublicFiles.ViewModels;

namespace CrisisManagement.Features.PublicFiles.Services;

public sealed class PublicFilesService(IUnitOfWork unitOfWork) : IPublicFilesService
{
    public Task<List<HelpFileViewModel>> GetHelpFilesAsync(CancellationToken ct) =>
        unitOfWork.Documents.GetHelpFilesAsync(ct);

    public async Task<HelpFileContentViewModel?> GetHelpFileAsync(int id, CancellationToken ct) =>
        (await unitOfWork.Documents.GetHelpFileAsync(id, ct))?.ToContentViewModel();
}
