namespace CrisisManagement.Features.Assessments.ViewModels;

// ---------------------------------------------------------------- Search Assessment

public sealed class AssessmentSearchRequest
{
    public int? ProviderId { get; set; }
    public string? ProviderPatientNo { get; set; }
    public string? Ssn { get; set; }
    public DateTime? AssessmentDateFrom { get; set; }
    public DateTime? AssessmentDateTo { get; set; }
    public string? LastName { get; set; }
    public string? FirstName { get; set; }
    public string? CompletedByLastName { get; set; }
    public string? CompletedByFirstName { get; set; }
    public int? F2FAssessmentId { get; set; }
    public int? PhoneAssessmentId { get; set; }
    public string? ProviderF2FAssessmentId { get; set; }
    public string? ProviderPhoneAssessmentId { get; set; }
    public bool IncompleteOnly { get; set; }   // the "tickler" list: dispatched calls and assessments not yet followed up
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public bool SortDesc { get; set; }
}

public sealed record AssessmentListItem(
    int F2FAssessmentId, int? PhoneAssessmentId, int PatientId, int ProviderId, DateTime AssessmentDate,
    string? LastName, string? FirstName, string? Ssn, DateTime? Dob, string? ProviderPatientId,
    string? ProviderName, string? Abbreviation, string? AssessmentType);

// ---------------------------------------------------------------- Display Files

public sealed class AssessmentFileSearchRequest
{
    public int? ProviderId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public bool SortDesc { get; set; }
}

public sealed record AssessmentFileListItem(
    int Id, string FileName, bool IsProcessed, int PhoneTotal, int PhoneImported, int PhoneErrors,
    int F2FTotal, int F2FImported, int F2FErrors, DateTime CreatedOn);

public sealed record FileUploadRaw(int Id, string FileName, string? Npi, string Xml);

public sealed record AssessmentFileErrorItem(long Id, string ErrorTable, int RecordId, string ErrorCode, DateTime CreatedOn);

public sealed record AssessmentUploadResult(int Id, string FileName, string? ProviderName);
