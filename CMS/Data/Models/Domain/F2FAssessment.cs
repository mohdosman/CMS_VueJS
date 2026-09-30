using System;
using System.Collections.Generic;

namespace CMS.Data.Models.Domain;

public sealed class F2FAssessment : AuditableEntity
{
    public int F2FAssessmentId { get; set; }

    public string? ProviderF2FAssessmentId { get; set; }

    public int? AssessmentTypeId { get; set; }

    public int PatientId { get; set; }

    public int ProviderId { get; set; }

    public int? PhoneAssessmentId { get; set; }

    public DateTime F2FAssessmentDate { get; set; }

    public int ResidentialStatusId { get; set; }

    public short CountyId { get; set; }

    public int EmploymentStatusId { get; set; }

    public int MaritalStatusId { get; set; }

    public int MilitaryStatusId { get; set; }

    public byte? Arrests30Days { get; set; }

    public byte School3MonthsId { get; set; }

    public int EducationLevelId { get; set; }

    public int PayorSourceId { get; set; }

    public int? SecondaryPayorSourceId { get; set; }

    public decimal? AnnualHouseholdIncome { get; set; }

    public byte? NumberInHousehold { get; set; }

    public int? CurrentServicesId { get; set; }

    public byte MHTreatmentDeclarationId { get; set; }

    public byte MOTStatusId { get; set; }

    public byte DurablePOAId { get; set; }

    public int AssessmentLocationtId { get; set; }

    public bool? TransportedByLE { get; set; }

    public bool? TelevideoAssessment { get; set; }

    public bool? CurrentDetoxWithdrawal { get; set; }

    public bool? HistoryDetoxWithdrawal { get; set; }

    public int? PrimaryMHSADiagnosisId { get; set; }

    public int? SecondaryMHSADiagnosisId { get; set; }

    public string CompletedByLastName { get; set; } = null!;

    public string CompletedByFirstName { get; set; } = null!;

    public DateTime TimeDispositionCompleted { get; set; }

    public DateTime? TimeTransported { get; set; }

    public byte? RecommendedTransportModeId { get; set; }

    public bool? FollowupContact { get; set; }

    public bool? FollowupReportedServiceHelpful { get; set; }

    public bool? VoluntaryAdmissionRecommended { get; set; }

    public bool? TelehealthAdmissionAssessment { get; set; }

    public byte? ContactAttempts { get; set; }

    public bool? IsAdmitted { get; set; }

    public byte? FirstHospitalizationId { get; set; }

    public short? PrimaryProblemId { get; set; }

    public byte? IntellectualDisabilityId { get; set; }

    public byte? MedicalInstabilityId { get; set; }

    public byte? MedicationIssuesId { get; set; }

    public byte? PastTraumaId { get; set; }

    public byte? SubstanceAbuseId { get; set; }

    public AssessmentLocation AssessmentLocationt { get; set; } = null!;

    public AssessmentType? AssessmentType { get; set; }

    public ICollection<F2FDrug> F2FDrugs { get; set; } = new List<F2FDrug>();

    public ICollection<F2FEvaluation> F2FEvaluations { get; set; } = new List<F2FEvaluation>();

    public ICollection<F2FHospAlternative> F2FHospAlternatives { get; set; } = new List<F2FHospAlternative>();

    public ICollection<F2FHospitalization> F2FHospitalizations { get; set; } = new List<F2FHospitalization>();

    public County County { get; set; } = null!;

    public CurrentService? CurrentServices { get; set; }

    public YesNoUnknown DurablePOA { get; set; } = null!;

    public EducationLevel EducationLevel { get; set; } = null!;

    public EmploymentStatus EmploymentStatus { get; set; } = null!;

    public YesNoUnknown? FirstHospitalization { get; set; }

    public YesNoUnknown? IntellectualDisability { get; set; }

    public YesNoUnknown MHTreatmentDeclaration { get; set; } = null!;

    public YesNoUnknown MOTStatus { get; set; } = null!;

    public MaritalStatus MaritalStatus { get; set; } = null!;

    public YesNoUnknown? MedicalInstability { get; set; }

    public YesNoUnknown? MedicationIssues { get; set; }

    public MilitaryStatus MilitaryStatus { get; set; } = null!;

    public YesNoUnknown? PastTrauma { get; set; }

    public Patient Patient { get; set; } = null!;

    public PayorSource PayorSource { get; set; } = null!;

    public PhoneAssessment? PhoneAssessment { get; set; }

    public PrimaryProblem? PrimaryProblem { get; set; }

    public Provider Provider { get; set; } = null!;

    public RecommendedTransportMode? RecommendedTransportMode { get; set; }

    public ResidentialStatus ResidentialStatus { get; set; } = null!;

    public YesNoUnknown School3Months { get; set; } = null!;

    public PayorSource? SecondaryPayorSource { get; set; }

    public YesNoUnknown? SubstanceAbuse { get; set; }
}

