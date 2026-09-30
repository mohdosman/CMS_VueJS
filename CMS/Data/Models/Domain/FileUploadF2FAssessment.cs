using System;
using System.Collections.Generic;

namespace CMS.Data.Models.Domain;

public sealed class FileUploadF2FAssessment : AuditableEntity
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

    public string? PrimaryMHSADiagnosis { get; set; }

    public string? SecondaryMHSADiagnosis { get; set; }

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

    public int FileUploadId { get; set; }

    public bool IsProcessed { get; set; }

    public int? ReferenceId { get; set; }

    public bool HasError { get; set; }

    public string? F2FAssessmentXML { get; set; }

    public bool IsImported { get; set; }

}

