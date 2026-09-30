using CrisisManagement.Features.Suicides.ViewModels;
using CrisisManagement.Shared.Common;

namespace CrisisManagement.Data.Repositories.Interfaces;

// Uploaded suicide (death record) files and the records read from them.
public interface ISuicideFileRepository
{
    // Name and content of the file (untracked); null when there is no such file.
    Task<SuicideFileDownload?> GetFileAsync(int id);

    // The records of one file. sortBy: lastName, firstName, ssn; anything else lists them in file order.
    Task<PagedResult<SuicideImportItem>> SearchImportsAsync(int fileId, int page, int size, string? sortBy, bool desc);

    // Runs usp_CMS_SuicideFileInsert, which stores the file and its records (a JSON array) in one transaction and
    // returns the new file id. The procedure raises error 51000 when it refuses the file.
    Task<int> InsertAsync(string fileName, byte[] content, string recordsJson, int recordCount, int userId);
}
