using CrisisManagement.Data;
using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Data.Repositories;
using CrisisManagement.Features.PublicFiles.Mapping;
using CrisisManagement.Features.PublicFiles.ViewModels;
using CrisisManagement.Shared.Common;

namespace CrisisManagement.Features.PublicFiles.Services;

public sealed class PublicFilesService(IUnitOfWork unitOfWork) : IPublicFilesService
{
    private const int MaxFileBytes = 5 * 1024 * 1024;   // same limit the Blazor CMS screen offered

    public Task<List<HelpFileViewModel>> GetHelpFilesAsync(CancellationToken ct) =>
        unitOfWork.Documents.GetHelpFilesAsync(ct);

    public async Task<HelpFileContentViewModel?> GetHelpFileAsync(int id, CancellationToken ct) =>
        (await unitOfWork.Documents.GetHelpFileAsync(id, ct))?.ToContentViewModel();

    public Task<PagedResult<HelpFileViewModel>> SearchAsync(PublicFileSearchRequest r) =>
        unitOfWork.Documents.SearchPublicFilesAsync(r.FileName, r.SortBy, r.SortDesc, Math.Max(1, r.PageIndex), Math.Clamp(r.PageSize, 1, 200));

    // A PDF only, up to 5 MB. It becomes an active help document, so it shows in the Help dialog straight away.
    public async Task<HelpFileViewModel> UploadAsync(string fileName, byte[] content)
    {
        var name = UploadChecks.RequirePdf(fileName, content, MaxFileBytes);

        var doc = new Document
        {
            FileName = name, FileContent = content, MIMEType = "application/pdf", IsActive = true,
            DocumentTypeId = DocumentRepository.HelpTypeId
        };
        unitOfWork.Documents.Add(doc);
        await unitOfWork.SaveChangesAsync();
        return new HelpFileViewModel(doc.DocumentId, doc.FileName, doc.CreatedOn, content.Length);
    }

    public async Task<bool> DeleteAsync(int id) => await unitOfWork.Documents.DeletePublicFileAsync(id) > 0;
}
