using CrisisManagement.Shared.Common;

namespace CrisisManagement.Features.Services.ViewModels;

// ---------------------------------------------------------------- Manage Service (search)

public sealed class ServiceSearchRequest
{
    public int? ProviderId { get; set; }
    public string? ProviderPatientNo { get; set; }
    public string? Ssn { get; set; }
    public string? LastName { get; set; }
    public string? FirstName { get; set; }
    public int? ServiceCodeId { get; set; }
    public DateTime? DosAdmitDateFrom { get; set; }
    public DateTime? DosAdmitDateTo { get; set; }
    public int? ServiceFileId { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public bool SortDesc { get; set; }
}

public sealed record ServiceListItem(
    int ServiceId, string? ProviderPatientNo, string? Ssn, string? LastName, string? FirstName, string? Dob,
    string? ProviderAbbrev, string? ServiceCodeAbbrev, string? DosAdmitDate, string? DischargeDate);

// ---------------------------------------------------------------- Enter/Edit Service

// One shape for the editor: sent on load and sent back on save. The patient block (provider patient number to gender)
// is only used when a service is created; an existing service keeps its patient.
public sealed class ServiceEditModel
{
    public int? Id { get; set; }
    public string? RowVersion { get; set; }
    public int? ProviderId { get; set; }

    public int? PatientId { get; set; }          // set when an existing patient was found for the provider patient number
    public string? ProviderPatientNo { get; set; }
    public string? Ssn { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? Dob { get; set; }
    public int? GenderId { get; set; }
    public int? CountyId { get; set; }           // county of residence

    public int? PayorSourceId { get; set; }      // payor billed for the service
    public int? PrimaryInsurerId { get; set; }
    public int? ServiceCodeId { get; set; }
    public DateTime? DosAdmitDate { get; set; }
    public DateTime? DischargeDate { get; set; }
    public int? DurationHours { get; set; }
    public int? ServiceCountyId { get; set; }
}

public sealed record ServiceCodeRule(int ServiceCodeId, bool IsDurationHoursRequired, bool IsDischargeDateRequired);

public sealed class ServiceLookups
{
    public List<LookupItem> Genders { get; set; } = [];
    public List<LookupItem> Counties { get; set; } = [];
    public List<LookupItem> PayorSources { get; set; } = [];
    public List<LookupItem> ServiceCodes { get; set; } = [];
    public List<ServiceCodeRule> ServiceCodeRules { get; set; } = [];
}

public sealed record ExistingPatient(int PatientId, string? ProviderPatientNo, string? Ssn, string? FirstName, string LastName, DateTime? Dob, int? GenderId);

// ---------------------------------------------------------------- Service files

public sealed class ServiceFileSearchRequest
{
    public int? ProviderId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public bool SortDesc { get; set; }
}

public sealed record ServiceFileListItem(
    int Id, string FileName, int? ServiceCount, int? PostedCount, int? ErrorCount, bool IsProcessed, bool IsInProcess, DateTime CreatedOn);

public sealed class ServiceFileErrorSearchRequest
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public bool SortDesc { get; set; }
}

public sealed record ServiceFileErrorItem(
    int Id, int ImportId, string? Ssn, string? Dob, string? FirstName, string? LastName, string? ServiceCode, string? DosAdmitDate, string? Description);

public sealed record ServiceFileRaw(int Id, string FileName, string Content);

public sealed record ServiceFileUploaded(int Id, string FileName, string ProviderName);
