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

    private const byte PhoneTable = 2, F2FTable = 3;   // CMS_FileUploadErrorTable ids

    // Like usp_CMS_DisplayErrors_rd: each error is joined to the staged assessment (and its patient) it points at.
    public async Task<List<AssessmentFileErrorItem>> GetErrorsAsync(int fileUploadId)
    {
        var errors = await context.Set<FileUploadError>().AsNoTracking().Where(e => e.FileUploadId == fileUploadId).OrderBy(e => e.FileUploadErrorId)
            .Select(e => new { e.FileUploadErrorId, Table = e.FileUploadErrorTable.Abbrev, Description = e.FileUploadErrorCode.FileUploadErrorCodeDescription, e.FileUploadErrorTableId, RecordId = e.FileUploadErrorTableRecordId })
            .ToListAsync();

        var phoneIds = errors.Where(e => e.FileUploadErrorTableId == PhoneTable).Select(e => e.RecordId).ToList();
        var f2fIds = errors.Where(e => e.FileUploadErrorTableId == F2FTable).Select(e => e.RecordId).ToList();
        var phones = await context.Set<FileUploadPhoneAssessment>().AsNoTracking().Where(p => phoneIds.Contains(p.PhoneAssessmentId))
            .Select(p => new { Id = p.PhoneAssessmentId, ProviderId = p.ProviderPhoneAssessmentId, Date = p.CallEnded, p.PatientId }).ToDictionaryAsync(p => p.Id);
        var f2fs = await context.Set<FileUploadF2FAssessment>().AsNoTracking().Where(f => f2fIds.Contains(f.F2FAssessmentId))
            .Select(f => new { Id = f.F2FAssessmentId, ProviderId = f.ProviderF2FAssessmentId, Date = f.F2FAssessmentDate, f.PatientId }).ToDictionaryAsync(f => f.Id);
        var patientIds = phones.Values.Select(p => p.PatientId).Concat(f2fs.Values.Select(f => f.PatientId)).Distinct().ToList();
        var patients = await context.Set<FileUploadPatient>().AsNoTracking().Where(p => patientIds.Contains(p.PatientId))
            .Select(p => new { p.PatientId, p.ProviderPatientNo }).ToDictionaryAsync(p => p.PatientId, p => p.ProviderPatientNo);

        string? PatientNo(int id) => patients.GetValueOrDefault(id);
        return errors.Select(e => e.FileUploadErrorTableId switch
        {
            PhoneTable when phones.TryGetValue(e.RecordId, out var p) => new AssessmentFileErrorItem(e.FileUploadErrorId, e.Table, p.ProviderId, PatientNo(p.PatientId), p.Date, e.Description),
            F2FTable when f2fs.TryGetValue(e.RecordId, out var f) => new AssessmentFileErrorItem(e.FileUploadErrorId, e.Table, f.ProviderId, PatientNo(f.PatientId), f.Date, e.Description),
            _ => new AssessmentFileErrorItem(e.FileUploadErrorId, e.Table, null, null, null, e.Description)
        }).ToList();
    }
}
