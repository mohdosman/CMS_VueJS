using CrisisManagement.Data.Context;
using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Data.Repositories.Interfaces;
using CrisisManagement.Features.Assessments.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CrisisManagement.Data.Repositories;

public sealed class FileUploadRepository(AppDbContext context) : Repository<FileUpload>(context), IFileUploadRepository
{
    public Task<bool> PendingExistsAsync(string fileName, string npi) =>
        _entities.AnyAsync(f => f.FileName == fileName && f.ProviderNPI == npi && !f.IsProcessed);

    public Task<FileUploadRaw?> GetRawAsync(int id) =>
        _entities.AsNoTracking().Where(f => f.FileUploadId == id)
            .Select(f => new FileUploadRaw(f.FileUploadId, f.FileName, f.ProviderNPI, f.FileTextXML)).FirstOrDefaultAsync();

    public Task<List<AssessmentFileErrorItem>> GetErrorsAsync(int fileUploadId) =>
        context.Set<FileUploadError>().AsNoTracking().Where(e => e.FileUploadId == fileUploadId)
            .OrderByDescending(e => e.CreatedOn)
            .Select(e => new AssessmentFileErrorItem(e.FileUploadErrorId, e.FileUploadErrorTable.Abbrev, e.FileUploadErrorTableRecordId,
                e.FileUploadErrorCode.FileUploadErrorCodeDescription, e.CreatedOn)).ToListAsync();
}
