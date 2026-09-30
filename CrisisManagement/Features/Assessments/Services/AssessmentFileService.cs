using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using CrisisManagement.Data;
using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Data.Models.SP;
using CrisisManagement.Data.StoredProcedures;
using CrisisManagement.Features.Assessments.ViewModels;
using CrisisManagement.Infrastructure.Identity;
using CrisisManagement.Shared.Common;
using CrisisManagement.Shared.Common.Files;

namespace CrisisManagement.Features.Assessments.Services;

// Assessment file upload (crisis assessment XML) and the Display Files screen. An uploaded file is only stored here; the
// nightly import job (usp_CMS_FileUploadImport) reads it later and records what it imported and which records failed.
public sealed class AssessmentFileService(IUnitOfWork uow, ProviderScope scope, IConfiguration config, ILogger<AssessmentFileService> log)
{
    private const int MaxFileBytes = 5 * 1024 * 1024;

    private string TargetNamespace => config["FileUpload:TargetNamespace"] ?? "http://www.tn.gov/mental/Schemas/CrisisAssessment";

    // The columns the procedure allows in ORDER BY (it lists processed files only, so there is no status filter).
    private static readonly Dictionary<string, string> SortColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        ["fileName"] = "FileName", ["isProcessed"] = "IsProcessed", ["createdOn"] = "CreatedOn",
        ["phoneTotal"] = "PATotalCount", ["phoneImported"] = "PACount", ["phoneErrors"] = "PAErrorCount",
        ["f2fTotal"] = "F2FTotalCount", ["f2fImported"] = "F2FCount", ["f2fErrors"] = "F2FErrorCount"
    };

    // ---------------------------------------------------------------- display files

    public async Task<PagedResult<AssessmentFileListItem>> SearchAsync(AssessmentFileSearchRequest r)
    {
        var errors = new ErrorBag();
        if (r.ProviderId is not > 0) errors.Add("providerId", "Provider is required.");
        if (r.DateFrom is { } from && r.DateTo is { } to && from > to)
            errors.Add("dateTo", "Date Processed(From) should be less than equal to Date Processed(To).");
        errors.ThrowIfAny();
        scope.Require(r.ProviderId!.Value);

        // This procedure converts dates as yyyy-MM-dd (the Blazor CMS sent MM/dd/yyyy, which it silently ignored).
        var prefs = new SearchPreferences()
            .Add("ProviderID", r.ProviderId)
            .Add("DateProcessedFrom", r.DateFrom?.ToString("yyyy-MM-dd"))
            .Add("DateProcessedTo", r.DateTo?.ToString("yyyy-MM-dd"));

        var column = r.SortBy is not null && SortColumns.TryGetValue(r.SortBy, out var c) ? c : "CreatedOn";
        var direction = r.SortBy is null || r.SortDesc ? "DESC" : "ASC";

        var (rows, total) = await uow.StoredProcedures.SearchAsync<UspFileSearchRd>(
            StoredProcedureConstants.QualifiedFileSearchRd, prefs, Math.Max(1, r.PageIndex), Math.Clamp(r.PageSize, 1, 200),
            $"[{column}] {direction}", x => x.TotalRowCount);

        return new PagedResult<AssessmentFileListItem>
        {
            TotalCount = total,
            Items = rows.Select(x => new AssessmentFileListItem(x.FileUploadId, x.FileName, x.IsProcessed, x.PATotalCount, x.PACount, x.PAErrorCount,
                x.F2FTotalCount, x.F2FCount, x.F2FErrorCount, x.CreatedOn)).ToList()
        };
    }

    // Null = no such file, or not one of the caller's providers: the two look the same on purpose.
    public async Task<FileUploadRaw?> GetRawAsync(int id)
    {
        var file = await uow.FileUploads.GetRawAsync(id);
        return file is not null && await CanSeeAsync(file.Npi) ? file : null;
    }

    public async Task<List<AssessmentFileErrorItem>?> GetErrorsAsync(int id) =>
        await GetRawAsync(id) is null ? null : await uow.FileUploads.GetErrorsAsync(id);

    private async Task<bool> CanSeeAsync(string? npi)
    {
        if (scope.IsAdmin) return true;
        var provider = string.IsNullOrWhiteSpace(npi) ? null : await uow.Providers.GetIdNameByNpiAsync(npi);
        return provider is not null && scope.Allows(provider.Id);
    }

    // ---------------------------------------------------------------- upload

    public async Task<AssessmentUploadResult> UploadAsync(string fileName, byte[] content)
    {
        var name = UploadChecks.Require(fileName, content, MaxFileBytes, "XML", FileSignatures.Xml);

        var doc = Parse(content);

        // A default namespace, when the file declares one, must be the schema's; a file without one is accepted and
        // treated as if it had it.
        var declared = doc.Root!.GetDefaultNamespace().NamespaceName;
        if (declared.Length > 0 && declared != TargetNamespace)
            throw Reject($"Invalid XML namespace '{declared}'. Expected '{TargetNamespace}'.");

        // The NPI comes from the file itself, never from the caller, so it cannot disagree with the content.
        var npi = FindNpi(doc) ?? throw Reject("NPI element not found in the XML file.");
        var provider = await uow.Providers.GetIdNameByNpiAsync(npi);
        if (provider is null || !scope.Allows(provider.Id)) throw Reject($"No provider found with NPI '{npi}'.");

        ValidateAgainstSchema(doc);

        if (await uow.FileUploads.PendingExistsAsync(name, npi))
            throw Reject($"A file named '{name}' is already pending for this provider.");

        // Stored without namespaces (and without an encoding declaration, which SQL Server's xml type rejects).
        foreach (var e in doc.Descendants()) e.Name = e.Name.LocalName;
        foreach (var a in doc.Descendants().Attributes().Where(a => a.IsNamespaceDeclaration || a.Name.Namespace != XNamespace.None).ToList()) a.Remove();   // xmlns and xsi:schemaLocation

        var entity = new FileUpload { FileName = name, FileTextXML = doc.Root!.ToString(SaveOptions.DisableFormatting), ProviderNPI = npi, IsInProcess = false, IsProcessed = false };
        uow.FileUploads.Add(entity);
        await uow.SaveChangesAsync();
        return new AssessmentUploadResult(entity.FileUploadId, name, provider.Name);
    }

    private static ValidationFailedException Reject(string message) => ValidationFailedException.For("file", message);

    // DTDs are refused and nothing external is fetched, so a hostile file cannot read files or call out.
    private static XDocument Parse(byte[] content)
    {
        try
        {
            using var stream = new MemoryStream(content);
            using var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });
            return XDocument.Load(reader);
        }
        catch (XmlException ex)
        {
            throw Reject($"Invalid XML format: {ex.Message}");
        }
    }

    // <Provider><NPI> (the schema root is Provider) or <Root><Provider><NPI>, whatever the namespace.
    private static string? FindNpi(XDocument doc)
    {
        bool IsNpi(XElement e) => e.Name.LocalName == "NPI";
        var root = doc.Root!;
        var e = root.Elements().FirstOrDefault(IsNpi)
            ?? root.Elements().Where(x => x.Name.LocalName == "Provider").SelectMany(x => x.Elements()).FirstOrDefault(IsNpi);
        return string.IsNullOrWhiteSpace(e?.Value) ? null : e.Value.Trim();
    }

    // Fails closed: without the schema file the upload is refused rather than accepted unchecked.
    private void ValidateAgainstSchema(XDocument doc)
    {
        var path = config["FileUpload:AssessmentSchemaPath"] ?? "Schemas/CrisisAssessment.xsd";
        if (!File.Exists(path))
        {
            log.LogError("Assessment schema file not found at {Path}; uploads are refused until it is available", path);
            throw Reject("The assessment schema file is not available on the server, so the file cannot be checked. Contact support.");
        }

        // A file without a namespace is checked as if it declared the schema's.
        var copy = new XDocument(doc);
        foreach (var e in copy.Descendants().Where(e => e.Name.NamespaceName.Length == 0).ToList()) e.Name = XNamespace.Get(TargetNamespace) + e.Name.LocalName;

        var messages = new List<string>();
        var schemas = new XmlSchemaSet { XmlResolver = null };
        try
        {
            using var xsd = XmlReader.Create(path, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });
            schemas.Add(TargetNamespace, xsd);
            copy.Validate(schemas, (_, e) => messages.Add(e.Message.Replace($"in namespace '{TargetNamespace}'", "").Replace($"'{TargetNamespace}:", "'")));
        }
        catch (XmlSchemaException ex)
        {
            log.LogError(ex, "The assessment schema could not be applied");
            throw Reject("The assessment schema could not be applied. Contact support.");
        }

        // The validator reports some problems twice; show each once.
        messages = messages.Select(m => Regex.Replace(m.Trim(), @"\s+", " ")).Distinct().ToList();
        if (messages.Count > 0)
            throw Reject("XML does not conform to the required schema. " + string.Join(" ", messages.Take(8)) + (messages.Count > 8 ? $" (+{messages.Count - 8} more)" : ""));
    }
}
