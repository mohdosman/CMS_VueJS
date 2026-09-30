namespace CrisisManagement.Data.Models.SP;

/// <summary>
/// Result row returned by dbo.usp_CMS_SearchAssessment_rd.
/// </summary>
public sealed class UspAssessmentSearchRd
{
    public int TotalRowCount { get; set; }
    public int F2FAssessmentId { get; set; }
    public int PatientId { get; set; }
    public int ProviderId { get; set; }
    public DateTime AssessmentDate { get; set; }
    public int? PhoneAssessmentId { get; set; }
    public string? ProviderPatientId { get; set; }
    public string? SSN { get; set; }
    public string? LastName { get; set; }
    public string? FirstName { get; set; }
    public DateTime? DOB { get; set; }
    public string? ProviderName { get; set; }
    public string? Abbreviation { get; set; }
    public string? EdisonNumber { get; set; }
    public bool IsActive { get; set; }
    public string? AssessmentType { get; set; }
}
