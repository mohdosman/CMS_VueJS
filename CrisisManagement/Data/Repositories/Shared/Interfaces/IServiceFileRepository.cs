using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Features.Services.ViewModels;

namespace CrisisManagement.Data.Repositories.Interfaces;

// Uploaded service files (CMS_ServiceFile). The nightly job imports their records and reports what failed.
public interface IServiceFileRepository : IRepository<ServiceFile>
{
    // A file with this name is already waiting to be processed for the provider.
    Task<bool> PendingExistsAsync(string fileName, int providerId);

    // Provider id of the file; null when there is no such file.
    Task<int?> GetProviderIdAsync(int id);

    // The file as text (untracked); null when there is no such file.
    Task<ServiceFileRaw?> GetRawAsync(int id);
}
