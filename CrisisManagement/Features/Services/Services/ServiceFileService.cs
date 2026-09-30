using System.Globalization;
using System.Text;
using System.Xml.Linq;
using CrisisManagement.Data;
using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Data.Models.SP;
using CrisisManagement.Data.StoredProcedures;
using CrisisManagement.Features.Services.ViewModels;
using CrisisManagement.Infrastructure.Identity;
using CrisisManagement.Shared.Common;

namespace CrisisManagement.Features.Services.Services;

// Service file upload (a comma-separated .txt file of service records) and the Display Service Files screen. An uploaded
// file is only checked and stored here; the nightly job imports its records and records what failed.
public sealed class ServiceFileService(IUnitOfWork uow, ProviderScope scope, ILogger<ServiceFileService> log)
{
    private const int MaxFileBytes = 5 * 1024 * 1024, FieldsPerRecord = 14, FieldsInHeader = 4;

    private static readonly Dictionary<string, string> FileSortColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        ["id"] = "ServiceFileId", ["fileName"] = "FileName", ["isProcessed"] = "IsProcessed", ["serviceCount"] = "ServiceCount",
        ["postedCount"] = "PostedCount", ["errorCount"] = "ErrorCount", ["createdOn"] = "CreatedOn"
    };

    private static readonly Dictionary<string, string> ErrorSortColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        ["id"] = "ServiceFileErrorId", ["importId"] = "ServiceFileImportId", ["ssn"] = "SSN", ["firstName"] = "FirstName",
        ["lastName"] = "LastName", ["serviceCode"] = "ServiceCode", ["dosAdmitDate"] = "DOSAdmitDate", ["description"] = "ServiceFileErrorCodeDescription"
    };

    // ---------------------------------------------------------------- display files

    public async Task<PagedResult<ServiceFileListItem>> SearchAsync(ServiceFileSearchRequest r)
    {
        var errors = new ErrorBag();
        if (r.ProviderId is not > 0) errors.Add("providerId", "Provider is required.");
        if (r.DateFrom is { } from && r.DateTo is { } to && from > to)
            errors.Add("dateTo", "Date Processed(From) should be less than equal to Date Processed(To).");
        errors.ThrowIfAny();
        scope.Require(r.ProviderId!.Value);

        var prefs = new SearchPreferences()
            .Add("ProviderID", r.ProviderId)
            .Add("DateProcessedFrom", r.DateFrom)
            .Add("DateProcessedTo", r.DateTo);

        var column = r.SortBy is not null && FileSortColumns.TryGetValue(r.SortBy, out var c) ? c : "CreatedOn";
        var direction = r.SortBy is null || r.SortDesc ? "DESC" : "ASC";

        var (rows, total) = await uow.StoredProcedures.SearchAsync<UspServiceFileSearchRd>(
            StoredProcedureConstants.QualifiedServiceFileSearchRd, prefs, Math.Max(1, r.PageIndex), Math.Clamp(r.PageSize, 1, 200),
            $"{column} {direction}", x => x.TotalRowCount);

        return new PagedResult<ServiceFileListItem>
        {
            TotalCount = total,
            Items = rows.Select(x => new ServiceFileListItem(x.ServiceFileId, x.FileName, x.ServiceCount, x.PostedCount, x.ErrorCount, x.IsProcessed, x.IsInProcess, x.CreatedOn)).ToList()
        };
    }

    // Null = no such file, or not one of the caller's providers: the two look the same on purpose.
    public async Task<ServiceFileRaw?> GetRawAsync(int id) => await CanSeeAsync(id) ? await uow.ServiceFiles.GetRawAsync(id) : null;

    // The import errors of a file. Null = no such file (or not the caller's). The procedure has no provider filter of its
    // own and lists every file's errors when the id is missing, so the scope check has to happen here.
    public async Task<PagedResult<ServiceFileErrorItem>?> SearchErrorsAsync(int serviceFileId, ServiceFileErrorSearchRequest r)
    {
        if (!await CanSeeAsync(serviceFileId)) return null;

        var column = r.SortBy is not null && ErrorSortColumns.TryGetValue(r.SortBy, out var c) ? c : "ServiceFileErrorId";
        var (rows, total) = await uow.StoredProcedures.SearchAsync<UspServiceFileErrorSearchRd>(
            StoredProcedureConstants.QualifiedServiceFileErrorSearchRd, new SearchPreferences().Add("ServiceFileId", serviceFileId),
            Math.Max(1, r.PageIndex), Math.Clamp(r.PageSize, 1, 200), $"{column} {(r.SortDesc ? "DESC" : "ASC")}", x => x.TotalRowCount);

        return new PagedResult<ServiceFileErrorItem>
        {
            TotalCount = total,
            Items = rows.Select(x => new ServiceFileErrorItem(x.ServiceFileErrorId, x.ServiceFileImportId, x.SSN, x.FirstName, x.LastName,
                x.ServiceCode, x.DOSAdmitDate, x.ServiceFileErrorCodeDescription)).ToList()
        };
    }

    private async Task<bool> CanSeeAsync(int id) => await uow.ServiceFiles.GetProviderIdAsync(id) is int providerId && scope.Allows(providerId);

    // ---------------------------------------------------------------- upload

    // Line 1: MMddyyyy,HHmm,ProviderName,ProviderNPI. Then one line per service record (14 fields). Last line: the record count.
    // Every problem found is reported, not only the first.
    public async Task<ServiceFileUploaded> UploadAsync(string fileName, byte[] content)
    {
        var name = UploadChecks.RequireBasics(fileName, content, MaxFileBytes);
        if (!name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            throw Reject($"Upload failed: Only TXT files are accepted. [{name}]");

        var text = ReadText(content);
        var lines = text.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0) throw Reject("File is empty or invalid.");

        var problems = new List<string>();

        // Footer: the number of service records.
        if (!int.TryParse(lines[^1], NumberStyles.None, CultureInfo.InvariantCulture, out var footer)) problems.Add("Invalid RowCount in Footer.");
        else if (footer != lines.Length - 2) problems.Add("RowCount is not equal to the number of service records.");

        // Header
        var header = lines[0].Split(',');
        if (header.Length != FieldsInHeader) throw Reject(problems.Append("Invalid Header.").ToArray());

        var providerName = header[2];
        var npi = header[3];
        if (!DateTime.TryParseExact($"{header[0]} {header[1]}", "MMddyyyy HHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var created))
            problems.Add("Invalid File Creation date.");
        if (npi.Length != 10 || !npi.All(char.IsAsciiDigit)) problems.Add("Provider NPI should be 10 digits.");

        var provider = npi.Length == 10 ? await uow.Providers.GetIdNameByNpiAsync(npi) : null;
        // A provider outside the caller's own is reported like an unknown one.
        if (provider is null || !scope.Allows(provider.Id)) problems.Add("Invalid Provider NPI.");

        var root = new XElement("ServiceRoot",
            new XElement("FileCreationDate", $"{header[0]} {header[1]}"), new XElement("ProviderName", providerName), new XElement("ProviderNPI", npi));
        string[] fields = ["ProviderPatientNo", "SSN", "DOB", "Gender", "FirstName", "LastName", "County", "ServiceCode", "DOSAdmitDate",
            "DischargeDate", "DurationHours", "PayorSource", "PrimaryInsurer", "ServiceCounty"];
        for (var i = 1; i < lines.Length - 1; i++)
        {
            var values = lines[i].Split(',');
            if (values.Length != FieldsPerRecord) { problems.Add($"Invalid Service Record at Line {i + 1}."); continue; }
            root.Add(new XElement("ServiceRecord", fields.Select((f, k) => new XElement(f, values[k]))));
        }

        if (problems.Count > 0) throw Reject(problems.ToArray());
        if (await uow.ServiceFiles.PendingExistsAsync(name, provider!.Id))
            throw Reject($"A file named '{name}' is already waiting to be processed for this provider.");

        var entity = new ServiceFile
        {
            FileName = name, ProviderId = provider.Id, ProviderNPI = npi, FileCreationDate = created,
            FileText = content, FileTextXml = root.ToString(SaveOptions.DisableFormatting)
        };
        uow.ServiceFiles.Add(entity);
        await uow.SaveChangesAsync();
        log.LogInformation("Service file {ServiceFileId} '{FileName}' stored for provider {ProviderId}", entity.ServiceFileId, name, provider.Id);
        return new ServiceFileUploaded(entity.ServiceFileId, name, provider.Name);
    }

    private static ValidationFailedException Reject(params string[] messages) => new(new() { ["file"] = messages });

    // UTF-8 (a leading byte order mark is dropped), or Latin-1 for the older files that are not; binary content and control
    // characters are not text.
    private static string ReadText(byte[] content)
    {
        string text;
        try { text = new UTF8Encoding(false, true).GetString(content).TrimStart((char)0xFEFF); }
        catch (DecoderFallbackException) { text = Encoding.Latin1.GetString(content); }
        if (text.Any(c => char.IsControl(c) && c != (char)13 && c != (char)10 && c != (char)9)) throw Reject("Invalid text file.");
        return text;
    }
}
