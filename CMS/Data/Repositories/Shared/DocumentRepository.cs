using CMS.Data.Context;
using CMS.Data.Models.Domain;
using CMS.Data.Repositories.Interfaces;
using CMS.Features.PublicFiles.ViewModels;
using CMS.Features.Users.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CMS.Data.Repositories;

public sealed class DocumentRepository(AppDbContext context) : Repository<Document>(context), IDocumentRepository
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

    public Task<bool> AnyForUserAsync(int userId) => _entities.AnyAsync(d => d.UserId == userId);

    public const int UserAgreementTypeId = 3;

    private IQueryable<Document> Agreements(int userId) =>
        _entities.Where(d => d.UserId == userId && d.DocumentTypeId == UserAgreementTypeId);

    public Task<List<UserDocumentItem>> GetUserAgreementsAsync(int userId) =>
        Agreements(userId).AsNoTracking().OrderByDescending(d => d.CreatedOn)
            .Select(d => new UserDocumentItem(d.DocumentId, d.FileName, d.CreatedOn, d.FileContent.Length)).ToListAsync();

    public Task<Document?> GetUserAgreementAsync(int userId, int documentId) =>
        Agreements(userId).AsNoTracking().FirstOrDefaultAsync(d => d.DocumentId == documentId);

    public Task<int> DeleteUserAgreementAsync(int userId, int documentId) =>
        Agreements(userId).Where(d => d.DocumentId == documentId).ExecuteDeleteAsync();
}
