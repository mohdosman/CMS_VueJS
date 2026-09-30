using CrisisManagement.Shared.Common;

namespace CrisisManagement.Features.Assessments.ViewModels;

// ---------------------------------------------------------------- Enter/Edit Assessment

public sealed class AssessmentDrugItem
{
    public int? DrugId { get; set; }
    public int? DrugRouteId { get; set; }
    public int? DrugFrequencyId { get; set; }
}

public sealed class AssessmentHospAltItem
{
    public int? HospitalizationAlternativeId { get; set; }
    public int? HospAltDispositionListId { get; set; }
}

public sealed class AssessmentHospitalizationItem
{
    public int? HospitalizationId { get; set; }
    public int? HospitalizationDispositionId { get; set; }
}

// The whole editor in one shape: the server sends it on load and the SPA sends it back on save. Dates that carry a time
// are local "yyyy-MM-ddTHH:mm" values (no time zone), as the database stores them.
public sealed class AssessmentEditModel
{
    // Which record this is: "f2f-<id>" or "pa-<id>"; empty for a new assessment. Set by the server, ignored on save.
    public string Key { get; set; } = "";
    public string? RowVersion { get; set; }
    public int? F2FAssessmentId { get; set; }
    public int? PhoneAssessmentId { get; set; }
    public string? ProviderF2FAssessmentId { get; set; }
    public string? ProviderPhoneAssessmentId { get; set; }
    public int? PatientId { get; set; }
    public int? ProviderId { get; set; }

    // Consumer
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Ssn { get; set; }
    public string? ProviderPatientNo { get; set; }
    public DateTime? Dob { get; set; }
    public int? GenderId { get; set; }
    public int? RaceId { get; set; }
    public int? EthnicityId { get; set; }

    // Crisis telephone
    public DateTime? CallEnded { get; set; }
    public DateTime? DispatchDateTime { get; set; }
    public int? DispositionId { get; set; }
    public string? DispositionOther { get; set; }
    public string? Notes { get; set; }

    // Crisis face to face
    public int? AssessmentTypeId { get; set; }
    public DateTime? F2FAssessmentDateTime { get; set; }
    public bool? TransportedByLE { get; set; }
    public int? PayorSourceId { get; set; }
    public int? SecondaryPayorSourceId { get; set; }
    public decimal? AnnualHouseholdIncome { get; set; }
    public int? NumberInHousehold { get; set; }
    public int? AssessmentLocationId { get; set; }
    public bool? TelevideoAssessment { get; set; }
    public int? CurrentServicesId { get; set; }
    public int? MHTreatmentDeclarationId { get; set; }
    public int? MOTStatusId { get; set; }
    public int? DurablePOAId { get; set; }
    public int? ResidentialStatusId { get; set; }
    public int? CountyId { get; set; }
    public int? EmploymentStatusId { get; set; }
    public int? Arrests30Days { get; set; }
    public int? MaritalStatusId { get; set; }
    public int? MilitaryStatusId { get; set; }
    public int? School3MonthsId { get; set; }
    public int? EducationLevelId { get; set; }
    public int? PrimaryProblemId { get; set; }
    public int? IntellectualDisabilityId { get; set; }
    public int? MedicalInstabilityId { get; set; }
    public int? MedicationIssuesId { get; set; }
    public int? PastTraumaId { get; set; }
    public int? SubstanceAbuseId { get; set; }
    public bool CurrentDetoxWithdrawal { get; set; }
    public bool HistoryDetoxWithdrawal { get; set; }
    public List<AssessmentDrugItem> Drugs { get; set; } = [];
    public List<AssessmentHospAltItem> HospAlternatives { get; set; } = [];
    public bool? VoluntaryAdmissionRecommended { get; set; }
    public bool? TelehealthAdmissionAssessment { get; set; }
    public int? FirstHospitalizationId { get; set; }
    public List<AssessmentHospitalizationItem> Hospitalizations { get; set; } = [];

    // Completion and follow-up
    public int? RecommendedTransportModeId { get; set; }
    public DateTime? TimeDispositionCompleted { get; set; }
    public DateTime? TimeTransported { get; set; }
    public string? CompletedByFirstName { get; set; }
    public string? CompletedByLastName { get; set; }
    public bool? FollowupContact { get; set; }
    public bool? IsAdmitted { get; set; }
    public bool? FollowupReportedServiceHelpful { get; set; }
    public int? ContactAttempts { get; set; }
}

// Label = description, Short = code/abbreviation (as in the other lookups).
public sealed record HospAltDispositionLookup(int HospitalizationAlternativeId, int DispositionListId, string Label);

public sealed class AssessmentLookups
{
    public List<LookupItem> Genders { get; set; } = [];
    public List<LookupItem> Races { get; set; } = [];
    public List<LookupItem> Ethnicities { get; set; } = [];
    public List<LookupItem> Dispositions { get; set; } = [];
    public List<LookupItem> AssessmentTypes { get; set; } = [];
    public List<LookupItem> AssessmentLocations { get; set; } = [];
    public List<LookupItem> PayorSources { get; set; } = [];
    public List<LookupItem> CurrentServices { get; set; } = [];
    public List<LookupItem> YesNoUnknown { get; set; } = [];
    public List<LookupItem> ResidentialStatuses { get; set; } = [];
    public List<LookupItem> Counties { get; set; } = [];
    public List<LookupItem> EmploymentStatuses { get; set; } = [];
    public List<LookupItem> MaritalStatuses { get; set; } = [];
    public List<LookupItem> MilitaryStatuses { get; set; } = [];
    public List<LookupItem> EducationLevels { get; set; } = [];
    public List<LookupItem> PrimaryProblems { get; set; } = [];
    public List<LookupItem> Drugs { get; set; } = [];
    public List<LookupItem> DrugRoutes { get; set; } = [];
    public List<LookupItem> DrugFrequencies { get; set; } = [];
    public List<LookupItem> HospitalizationAlternatives { get; set; } = [];
    public List<HospAltDispositionLookup> HospAltDispositions { get; set; } = [];
    public List<LookupItem> Hospitalizations { get; set; } = [];
    public List<LookupItem> HospitalizationDispositions { get; set; } = [];
    public List<LookupItem> TransportModes { get; set; } = [];
}

public sealed record AssessmentSaved(string Key);
