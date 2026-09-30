using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Features.Assessments.ViewModels;

namespace CrisisManagement.Data.Repositories.Interfaces;

// Uploaded assessment XML files (CMS_FileUpload) and the errors the import job recorded for them.
public interface IFileUploadRepository : IRepository<FileUpload>
{
    // A file with this name is already waiting to be processed for the provider NPI.
    Task<bool> PendingExistsAsync(string fileName, string npi);

    // File name, NPI and the XML text; null when there is no such file (untracked).
    Task<FileUploadRaw?> GetRawAsync(int id);

    // The import errors of a file, newest first.
    Task<List<AssessmentFileErrorItem>> GetErrorsAsync(int fileUploadId);
}
