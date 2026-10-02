using CrisisManagement.Data.Context;
using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Data.Repositories.Interfaces;
using CrisisManagement.Features.PublicFiles.ViewModels;
using CrisisManagement.Features.Users.ViewModels;
using CrisisManagement.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace CrisisManagement.Data.Repositories;

public sealed class DocumentRepository(AppDbContext context) : Repository<Document>(context), IDocumentRepository
{
    public const int HelpTypeId = 2;   // CMS_DocumentType: Help (3 = user agreement)

    private IQueryable<Document> HelpDocuments =>
        _entities.AsNoTracking().Where(d => d.DocumentTypeId == HelpTypeId && d.UserId == null && d.IsActive);

    public Task<List<HelpFileViewModel>> GetHelpFilesAsync(CancellationToken ct = default) =>
        HelpDocuments.OrderByDescending(d => d.CreatedOn)
            .Select(d => new HelpFileViewModel(d.DocumentId, d.FileName, d.CreatedOn, d.FileContent.Length))
            .ToListAsync(ct);

    public Task<Document?> GetHelpFileAsync(int id, CancellationToken ct = default) =>
        HelpDocuments.Where(d => d.DocumentId == id).FirstOrDefaultAsync(ct);

    public Task<bool> AnyForUserAsync(int userId) => _entities.AnyAsync(d => d.UserId == userId);

    public async Task<PagedResult<HelpFileViewModel>> SearchPublicFilesAsync(string? fileName, string? sortBy, bool desc, int page, int size)
    {
        var q = HelpDocuments;
        if (!string.IsNullOrWhiteSpace(fileName)) { var v = LikePattern.Contains(fileName!); q = q.Where(d => EF.Functions.Like(d.FileName, v, LikePattern.Escape)); }

        q = (sortBy ?? "").ToLowerInvariant() switch
        {
            "id" => desc ? q.OrderByDescending(d => d.DocumentId) : q.OrderBy(d => d.DocumentId),
            "filename" => desc ? q.OrderByDescending(d => d.FileName) : q.OrderBy(d => d.FileName),
            "filesize" => desc ? q.OrderByDescending(d => d.FileContent.Length) : q.OrderBy(d => d.FileContent.Length),
            _ => sortBy is null || desc ? q.OrderByDescending(d => d.CreatedOn) : q.OrderBy(d => d.CreatedOn)
        };

        var total = await q.CountAsync();
        var items = await q.Skip((page - 1) * size).Take(size)
            .Select(d => new HelpFileViewModel(d.DocumentId, d.FileName, d.CreatedOn, d.FileContent.Length)).ToListAsync();
        return new PagedResult<HelpFileViewModel> { Items = items, TotalCount = total };
    }

    public Task<int> DeletePublicFileAsync(int id) =>
        _entities.Where(d => d.DocumentId == id && d.DocumentTypeId == HelpTypeId && d.UserId == null).ExecuteDeleteAsync();

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
