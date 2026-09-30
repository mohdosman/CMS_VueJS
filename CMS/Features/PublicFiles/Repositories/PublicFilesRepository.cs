using CMS.Data.Context;
using CMS.Data.Models.Domain;
using CMS.Data.Repositories;
using CMS.Features.PublicFiles.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CMS.Features.PublicFiles.Repositories;

public sealed class PublicFilesRepository(AppDbContext context) : Repository<Document>(context), IPublicFilesRepository
{
    private const int HelpTypeId = 2;   // CMS_DocumentType: Help (3 = user agreement)

    private IQueryable<Document> HelpDocuments =>
        _entities.AsNoTracking().Where(d => d.DocumentTypeId == HelpTypeId && d.UserId == null && d.IsActive);

    public Task<List<HelpFileViewModel>> GetHelpFilesAsync(CancellationToken ct = default) =>
        HelpDocuments.OrderByDescending(d => d.CreatedOn)
            .Select(d => new HelpFileViewModel(d.DocumentId, d.FileName, d.CreatedOn, d.FileContent.Length))
            .ToListAsync(ct);

    public Task<Document?> GetHelpFileAsync(int id, CancellationToken ct = default) =>
        HelpDocuments.Where(d => d.DocumentId == id).FirstOrDefaultAsync(ct);
}
