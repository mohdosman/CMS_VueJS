using CrisisManagement.Data;
using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.SP;
using CrisisManagement.Data.StoredProcedures;
using CrisisManagement.Features.Suicides.ViewModels;
using CrisisManagement.Shared.Common;
using CrisisManagement.Shared.Common.Files;
using CrisisManagement.Shared.Extensions;
using Microsoft.Data.SqlClient;

namespace CrisisManagement.Features.Suicides.Services;

// Suicide (death record) file upload and the Display Suicide Files screen. These files are not tied to a provider, so access
// is by permission only.
public sealed class SuicideFileService(IUnitOfWork uow, IHttpContextAccessor http, ILogger<SuicideFileService> log)
{
    private const int MaxFileBytes = 5 * 1024 * 1024, MaxPageSize = 200;

    private static readonly Dictionary<string, string> SortColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        ["id"] = "SuicideFileId", ["fileName"] = "FileName", ["recordCount"] = "RecordCount", ["createdOn"] = "CreatedOn"
    };

    public async Task<PagedResult<SuicideFileListItem>> SearchAsync(SuicideFileSearchRequest r)
    {
        var errors = new ErrorBag();
        if (r.DateFrom is { } from && r.DateTo is { } to && from > to)
            errors.Add("dateTo", "Date Processed(From) should be less than equal to Date Processed(To).");
        // The procedure builds its query by string concatenation: quotes are doubled and the value has to fit its variable.
        if (TextSafety.Problem(r.FileName, "File Name") is { } problem) errors.Add("fileName", problem);
        errors.ThrowIfAny();

        var prefs = new SearchPreferences().AddEscaped("FileName", r.FileName).Add("DateProcessedFrom", r.DateFrom).Add("DateProcessedTo", r.DateTo);
        var column = r.SortBy is not null && SortColumns.TryGetValue(r.SortBy, out var c) ? c : "CreatedOn";
        var direction = r.SortBy is null || r.SortDesc ? "DESC" : "ASC";

        var (rows, total) = await uow.StoredProcedures.SearchAsync<UspSuicideFileSearchRd>(
            StoredProcedureConstants.QualifiedSuicideFileSearchRd, prefs, Math.Max(1, r.PageIndex), Math.Clamp(r.PageSize, 1, MaxPageSize),
            $"{column} {direction}", x => x.TotalRowCount);

        return new PagedResult<SuicideFileListItem>
        {
            TotalCount = total,
            Items = rows.Select(x => new SuicideFileListItem(x.SuicideFileId, x.FileName, x.FileSize, x.RecordCount, x.CreatedOn)).ToList()
        };
    }

    public Task<PagedResult<SuicideImportItem>> SearchImportsAsync(int fileId, SuicideImportSearchRequest r) =>
        uow.SuicideFiles.SearchImportsAsync(fileId, Math.Max(1, r.PageIndex), Math.Clamp(r.PageSize, 1, MaxPageSize), r.SortBy, r.SortDesc);

    // Null = no such file.
    public Task<SuicideFileDownload?> DownloadAsync(int id) => uow.SuicideFiles.GetFileAsync(id);

    public async Task<SuicideFileUploaded> UploadAsync(string fileName, byte[] content)
    {
        var name = UploadChecks.Require(fileName, content, MaxFileBytes, ".xlsx Excel", FileSignatures.Excel);

        var (result, problems) = SuicideWorkbookReader.Read(content);
        if (result is null) throw Reject(problems.ToArray());

        try
        {
            var id = await uow.SuicideFiles.InsertAsync(name, content, result.Json, result.RecordCount, http.HttpContext!.User.GetUserId());
            log.LogInformation("Suicide file {SuicideFileId} '{FileName}' stored with {Count} records", id, name, result.RecordCount);
            return new SuicideFileUploaded(id, name, result.RecordCount);
        }
        catch (SqlException ex) when (ex.Number == 51000)
        {
            // The procedure's own message is shown only for the check it makes on purpose.
            log.LogWarning(ex, "usp_CMS_SuicideFileInsert refused '{FileName}': {Message}", name, ex.Message);
            throw Reject(ex.Message == "Error! Count does not match." ? ex.Message : "The file could not be saved. Check that its columns and values match the template.");
        }
    }

    private static ValidationFailedException Reject(params string[] messages) => new(new() { ["file"] = messages });
}
