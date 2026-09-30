using System.Text.RegularExpressions;
using CrisisManagement.Data;
using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Features.Assessments.ViewModels;
using CrisisManagement.Infrastructure.Identity;
using CrisisManagement.Shared.Common;
using CrisisManagement.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace CrisisManagement.Features.Assessments.Services;

// Enter/Edit Assessment: a consumer (patient) with an optional crisis telephone assessment and an optional face to face
// (F2F) assessment. A record is addressed by a key, "f2f-<id>" or "pa-<id>". Everything is limited to the caller's providers.
public sealed partial class AssessmentEditorService(IUnitOfWork uow, ProviderScope scope)
{
    private const int MobileCrisisDispatched = 4, OtherDisposition = 6, Yes = 1, ReferralAccepted = 1;
    private static readonly DateTime DobCutoff = new(2014, 7, 1);   // a DOB is required for assessments from this date on

    [GeneratedRegex(@"^(f2f|pa)-(\d+)$")] private static partial Regex KeyShape();

    private static bool TryParseKey(string key, out bool isF2F, out int id)
    {
        var m = KeyShape().Match(key ?? "");
        isF2F = m.Success && m.Groups[1].Value == "f2f";
        id = m.Success && int.TryParse(m.Groups[2].Value, out var n) ? n : 0;
        return m.Success && id > 0;
    }

    public Task<AssessmentLookups> GetLookupsAsync() => uow.Assessments.GetLookupsAsync();

    // ---------------------------------------------------------------- load

    // Null = no such assessment. A phone call that already produced a F2F opens as that F2F.
    public async Task<AssessmentEditModel?> GetAsync(string key)
    {
        if (!TryParseKey(key, out var isF2F, out var id)) return null;

        if (!isF2F)
        {
            var phone = await uow.Assessments.GetPhoneAsync(id);
            if (phone is null) return null;
            scope.Require(phone.ProviderId);
            var latest = await uow.Assessments.GetLatestF2FIdAsync(id);
            if (latest is null) return ToModel(null, phone);
            id = latest.Value;
        }

        var f2f = await uow.Assessments.GetF2FAsync(id);
        if (f2f is null) return null;
        scope.Require(f2f.ProviderId);
        return ToModel(f2f, f2f.PhoneAssessment);
    }

    private static AssessmentEditModel ToModel(F2FAssessment? f2f, PhoneAssessment? phone)
    {
        var p = f2f?.Patient ?? phone!.Patient;
        var m = new AssessmentEditModel
        {
            Key = f2f is not null ? $"f2f-{f2f.F2FAssessmentId}" : $"pa-{phone!.PhoneAssessmentId}",
            RowVersion = Convert.ToBase64String(f2f?.Version ?? phone!.Version),
            F2FAssessmentId = f2f?.F2FAssessmentId,
            PhoneAssessmentId = phone?.PhoneAssessmentId,
            ProviderF2FAssessmentId = f2f?.ProviderF2FAssessmentId,
            ProviderPhoneAssessmentId = phone?.ProviderPhoneAssessmentId,
            PatientId = p.PatientId,
            ProviderId = f2f?.ProviderId ?? phone!.ProviderId,
            FirstName = p.FirstName, LastName = p.LastName, Ssn = p.SSN, ProviderPatientNo = p.ProviderPatientNo, Dob = p.DOB,
            GenderId = p.GenderId, RaceId = p.RaceId, EthnicityId = p.EthnicityId
        };
        if (phone is not null)
        {
            m.CallEnded = phone.CallEnded;
            m.DispatchDateTime = phone.DispositionDispatchTime;
            m.DispositionId = phone.DispositionId;
            m.DispositionOther = phone.DispositionOther;
            m.Notes = phone.Comment;
        }
        if (f2f is null) return m;

        m.AssessmentTypeId = f2f.AssessmentTypeId;
        m.F2FAssessmentDateTime = f2f.F2FAssessmentDate;
        m.TransportedByLE = f2f.TransportedByLE;
        m.PayorSourceId = f2f.PayorSourceId;
        m.SecondaryPayorSourceId = f2f.SecondaryPayorSourceId;
        m.AnnualHouseholdIncome = f2f.AnnualHouseholdIncome;
        m.NumberInHousehold = f2f.NumberInHousehold;
        m.AssessmentLocationId = f2f.AssessmentLocationtId;
        m.TelevideoAssessment = f2f.TelevideoAssessment;
        m.CurrentServicesId = f2f.CurrentServicesId;
        m.MHTreatmentDeclarationId = f2f.MHTreatmentDeclarationId;
        m.MOTStatusId = f2f.MOTStatusId;
        m.DurablePOAId = f2f.DurablePOAId;
        m.ResidentialStatusId = f2f.ResidentialStatusId;
        m.CountyId = f2f.CountyId;
        m.EmploymentStatusId = f2f.EmploymentStatusId;
        m.Arrests30Days = f2f.Arrests30Days;
        m.MaritalStatusId = f2f.MaritalStatusId;
        m.MilitaryStatusId = f2f.MilitaryStatusId;
        m.School3MonthsId = f2f.School3MonthsId;
        m.EducationLevelId = f2f.EducationLevelId;
        m.PrimaryProblemId = f2f.PrimaryProblemId;
        m.IntellectualDisabilityId = f2f.IntellectualDisabilityId;
        m.MedicalInstabilityId = f2f.MedicalInstabilityId;
        m.MedicationIssuesId = f2f.MedicationIssuesId;
        m.PastTraumaId = f2f.PastTraumaId;
        m.SubstanceAbuseId = f2f.SubstanceAbuseId;
        m.CurrentDetoxWithdrawal = f2f.CurrentDetoxWithdrawal ?? false;
        m.HistoryDetoxWithdrawal = f2f.HistoryDetoxWithdrawal ?? false;
        m.Drugs = f2f.F2FDrugs.Select(d => new AssessmentDrugItem { DrugId = d.DrugId, DrugRouteId = d.DrugRouteId, DrugFrequencyId = d.DrugFrequencyId }).ToList();
        m.HospAlternatives = f2f.F2FHospAlternatives.Select(h => new AssessmentHospAltItem
            { HospitalizationAlternativeId = h.HospAltDisposition.HospitalizationAlternativeId, HospAltDispositionListId = h.HospAltDisposition.HospAltDispositionListId }).ToList();
        m.VoluntaryAdmissionRecommended = f2f.VoluntaryAdmissionRecommended;
        m.TelehealthAdmissionAssessment = f2f.TelehealthAdmissionAssessment;
        m.FirstHospitalizationId = f2f.FirstHospitalizationId;
        m.Hospitalizations = f2f.F2FHospitalizations.Select(h => new AssessmentHospitalizationItem
            { HospitalizationId = h.HospitalizationId, HospitalizationDispositionId = h.HospitalizationDispositionId }).ToList();
        m.RecommendedTransportModeId = f2f.RecommendedTransportModeId;
        m.TimeDispositionCompleted = f2f.TimeDispositionCompleted;
        m.TimeTransported = f2f.TimeTransported;
        m.CompletedByFirstName = f2f.CompletedByFirstName;
        m.CompletedByLastName = f2f.CompletedByLastName;
        m.FollowupContact = f2f.FollowupContact;
        m.IsAdmitted = f2f.IsAdmitted;
        m.FollowupReportedServiceHelpful = f2f.FollowupReportedServiceHelpful;
        m.ContactAttempts = f2f.ContactAttempts;
        return m;
    }

    // ---------------------------------------------------------------- save

    public async Task<AssessmentSaved> CreateAsync(AssessmentEditModel m)
    {
        if (m.ProviderId is not > 0)
        {
            // Name every other problem as well, so the form shows them all at once.
            var errors = new ErrorBag();
            errors.Add("providerId", "Please select the Provider!");
            var isF2F = m.F2FAssessmentDateTime is not null;
            Validate(m, errors, isF2F, !isF2F || m.CallEnded is not null || m.DispositionId is not null || m.DispatchDateTime is not null);
            errors.ThrowIfAny();
        }
        scope.Require(m.ProviderId!.Value);
        return await SaveAsync(m, m.ProviderId.Value, null, null, new Patient());
    }

    // Null = no such assessment.
    public async Task<AssessmentSaved?> UpdateAsync(string key, AssessmentEditModel m)
    {
        if (!TryParseKey(key, out var isF2F, out var id)) return null;

        F2FAssessment? f2f = null;
        PhoneAssessment? phone;
        Patient patient;
        if (isF2F)
        {
            f2f = await uow.Assessments.GetF2FAsync(id);
            if (f2f is null) return null;
            phone = f2f.PhoneAssessment;
            patient = f2f.Patient;
        }
        else
        {
            phone = await uow.Assessments.GetPhoneAsync(id);
            if (phone is null) return null;
            patient = phone.Patient;
        }

        var providerId = f2f?.ProviderId ?? phone!.ProviderId;
        scope.Require(providerId);
        if (m.ProviderId is > 0 && m.ProviderId != providerId)
            throw ValidationFailedException.For("providerId", "The provider of an existing assessment cannot be changed.");

        // Compared here rather than through EF's original value, which a reload would overwrite.
        var current = f2f?.Version ?? phone!.Version;
        if (!string.IsNullOrEmpty(m.RowVersion) && !current.AsSpan().SequenceEqual(Convert.FromBase64String(m.RowVersion)))
            throw new ConflictException("This assessment was changed by someone else. Reload the page and try again.");

        return await SaveAsync(m, providerId, f2f, phone, patient);
    }

    // Deleting a F2F leaves its phone call and the patient; deleting a phone call takes its F2Fs with it. The patient always stays.
    public async Task<bool> DeleteAsync(string key)
    {
        if (!TryParseKey(key, out var isF2F, out var id)) return false;

        if (isF2F)
        {
            var f2f = await uow.Assessments.GetF2FAsync(id);
            if (f2f is null) return false;
            scope.Require(f2f.ProviderId);
            uow.Assessments.RemoveF2F(f2f);
        }
        else
        {
            var phone = await uow.Assessments.GetPhoneWithF2FsAsync(id);
            if (phone is null) return false;
            scope.Require(phone.ProviderId);
            foreach (var f in phone.F2FAssessments.ToList()) uow.Assessments.RemoveF2F(f);
            uow.Assessments.RemovePhone(phone);
        }
        await SaveChangesAsync();
        return true;
    }

    private async Task<AssessmentSaved> SaveAsync(AssessmentEditModel m, int providerId, F2FAssessment? f2f, PhoneAssessment? phone, Patient patient)
    {
        var isF2F = f2f is not null || m.F2FAssessmentDateTime is not null;
        // A phone panel that is filled in (or that already has a row, or the only thing being entered) is validated and saved.
        var wantsPhone = phone is not null || !isF2F || m.CallEnded is not null || m.DispositionId is not null || m.DispatchDateTime is not null;

        // Rows the user left completely blank in a grid are not rows.
        m.Drugs.RemoveAll(d => d.DrugId is null && d.DrugRouteId is null && d.DrugFrequencyId is null);
        m.HospAlternatives.RemoveAll(a => a.HospitalizationAlternativeId is null && a.HospAltDispositionListId is null);
        m.Hospitalizations.RemoveAll(h => h.HospitalizationId is null && h.HospitalizationDispositionId is null);

        var errors = new ErrorBag();
        Validate(m, errors, isF2F, wantsPhone);
        var lastName = m.LastName?.Trim() ?? "";
        if (wantsPhone && m.CallEnded is { } call && !errors.Has("callEnded") &&
            await uow.Assessments.PhoneDuplicateExistsAsync(phone?.PhoneAssessmentId ?? 0, providerId, call, lastName))
            errors.Add("callEnded", "Duplicate phone assessments with the same: Call Ended Date/Time, Last Name & Provider.");
        if (isF2F && m.F2FAssessmentDateTime is { } seen && !errors.Has("f2FAssessmentDateTime") &&
            await uow.Assessments.F2FDuplicateExistsAsync(f2f?.F2FAssessmentId ?? 0, providerId, seen, lastName))
            errors.Add("f2FAssessmentDateTime", "Duplicate F2F assessments with the same: Assessment Date, Arrival Time, Last Name & Provider.");
        var altMap = await uow.Assessments.GetHospAltDispositionMapAsync();
        foreach (var a in m.HospAlternatives)
            if (a.HospitalizationAlternativeId is int alt && a.HospAltDispositionListId is int list && !altMap.ContainsKey((alt, list)))
                errors.Add("hospAlternatives", "The disposition is not valid for the selected alternative.");
        errors.ThrowIfAny();

        ApplyPatient(patient, m);
        if (patient.PatientId == 0) uow.Assessments.AddPatient(patient);

        if (wantsPhone)
        {
            var isNew = phone is null;
            phone ??= new PhoneAssessment { ProviderId = providerId };
            phone.Patient = patient;
            phone.CallEnded = m.CallEnded!.Value;
            phone.DispositionId = m.DispositionId!.Value;
            // These only exist for their own dispositions; stale values sent with another one are not stored.
            phone.DispositionOther = m.DispositionId == OtherDisposition ? Blank(m.DispositionOther) : null;
            phone.DispositionDispatchTime = m.DispositionId == MobileCrisisDispatched ? m.DispatchDateTime : null;
            phone.Comment = Blank(m.Notes);
            if (isNew) uow.Assessments.AddPhone(phone);
        }

        if (isF2F)
        {
            var isNew = f2f is null;
            f2f ??= new F2FAssessment { ProviderId = providerId };
            f2f.Patient = patient;
            f2f.PhoneAssessment = phone;
            ApplyF2F(f2f, m);
            if (isNew) uow.Assessments.AddF2F(f2f);
            else uow.Assessments.RemoveF2FChildren(f2f);
            AddChildren(f2f, m, altMap);
        }

        await SaveChangesAsync();
        return new AssessmentSaved(f2f is not null ? $"f2f-{f2f.F2FAssessmentId}" : $"pa-{phone!.PhoneAssessmentId}");
    }

    // A foreign-key failure means the request named a lookup value that does not exist.
    private async Task SaveChangesAsync()
    {
        try { await uow.SaveChangesAsync(); }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase) == true)
        {
            throw ValidationFailedException.For("form", "One of the selected values is not valid. Reload the page and try again.");
        }
    }

    private static string? Blank(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();

    private static byte B(int? v) => v is >= 0 and <= 255 ? (byte)v : throw ValidationFailedException.For("form", "One of the selected values is not valid.");
    private static byte? BN(int? v) => v is null ? null : B(v);

    private static void ApplyPatient(Patient p, AssessmentEditModel m)
    {
        p.FirstName = m.FirstName!.Trim();
        p.LastName = m.LastName!.Trim();
        p.SSN = SsnPolicy.TryNormalize(m.Ssn, out var digits) ? digits : null;
        p.ProviderPatientNo = Blank(m.ProviderPatientNo);
        p.DOB = m.Dob?.Date;
        p.GenderId = BN(m.GenderId);
        p.RaceId = BN(m.RaceId);
        p.EthnicityId = BN(m.EthnicityId);
    }

    private static void ApplyF2F(F2FAssessment f, AssessmentEditModel m)
    {
        f.AssessmentTypeId = m.AssessmentTypeId;
        f.F2FAssessmentDate = m.F2FAssessmentDateTime!.Value;
        f.TransportedByLE = m.TransportedByLE;
        f.PayorSourceId = m.PayorSourceId!.Value;
        f.SecondaryPayorSourceId = m.SecondaryPayorSourceId;
        f.AnnualHouseholdIncome = m.AnnualHouseholdIncome;
        f.NumberInHousehold = BN(m.NumberInHousehold);
        f.AssessmentLocationtId = m.AssessmentLocationId!.Value;
        f.TelevideoAssessment = m.TelevideoAssessment;
        f.CurrentServicesId = m.CurrentServicesId;
        f.MHTreatmentDeclarationId = B(m.MHTreatmentDeclarationId);
        f.MOTStatusId = B(m.MOTStatusId);
        f.DurablePOAId = B(m.DurablePOAId);
        f.ResidentialStatusId = m.ResidentialStatusId!.Value;
        f.CountyId = m.CountyId is > 0 and <= short.MaxValue ? (short)m.CountyId : throw ValidationFailedException.For("form", "One of the selected values is not valid.");
        f.EmploymentStatusId = m.EmploymentStatusId!.Value;
        f.Arrests30Days = BN(m.Arrests30Days);
        f.MaritalStatusId = m.MaritalStatusId!.Value;
        f.MilitaryStatusId = m.MilitaryStatusId!.Value;
        f.School3MonthsId = B(m.School3MonthsId);
        f.EducationLevelId = m.EducationLevelId!.Value;
        f.PrimaryProblemId = m.PrimaryProblemId is null ? null : m.PrimaryProblemId is >= 0 and <= short.MaxValue ? (short)m.PrimaryProblemId : throw ValidationFailedException.For("form", "One of the selected values is not valid.");
        f.IntellectualDisabilityId = BN(m.IntellectualDisabilityId);
        f.MedicalInstabilityId = BN(m.MedicalInstabilityId);
        f.MedicationIssuesId = BN(m.MedicationIssuesId);
        f.PastTraumaId = BN(m.PastTraumaId);
        f.SubstanceAbuseId = BN(m.SubstanceAbuseId);
        f.CurrentDetoxWithdrawal = m.CurrentDetoxWithdrawal;
        f.HistoryDetoxWithdrawal = m.HistoryDetoxWithdrawal;
        f.VoluntaryAdmissionRecommended = m.VoluntaryAdmissionRecommended;
        f.TelehealthAdmissionAssessment = m.TelehealthAdmissionAssessment;
        f.FirstHospitalizationId = BN(m.FirstHospitalizationId);
        f.RecommendedTransportModeId = BN(m.RecommendedTransportModeId);
        f.TimeDispositionCompleted = m.TimeDispositionCompleted!.Value;
        f.TimeTransported = m.TimeTransported;
        f.CompletedByFirstName = m.CompletedByFirstName!.Trim();
        f.CompletedByLastName = m.CompletedByLastName!.Trim();
        f.FollowupContact = m.FollowupContact;
        f.IsAdmitted = m.IsAdmitted;
        f.FollowupReportedServiceHelpful = m.FollowupReportedServiceHelpful;
        f.ContactAttempts = BN(m.ContactAttempts);
    }

    private static void AddChildren(F2FAssessment f, AssessmentEditModel m, Dictionary<(int Alternative, int List), int> altMap)
    {
        foreach (var d in m.Drugs)
            f.F2FDrugs.Add(new F2FDrug { DrugId = d.DrugId!.Value, DrugRouteId = d.DrugRouteId, DrugFrequencyId = d.DrugFrequencyId });
        foreach (var a in m.HospAlternatives)
            f.F2FHospAlternatives.Add(new F2FHospAlternative { HospAltDispositionId = altMap[(a.HospitalizationAlternativeId!.Value, a.HospAltDispositionListId!.Value)] });
        foreach (var h in m.Hospitalizations)
            f.F2FHospitalizations.Add(new F2FHospitalization { HospitalizationId = h.HospitalizationId!.Value, HospitalizationDispositionId = h.HospitalizationDispositionId!.Value });
    }

    // ---------------------------------------------------------------- validation (the Blazor CMS rules and messages)

    private static void Validate(AssessmentEditModel m, ErrorBag e, bool isF2F, bool wantsPhone)
    {
        var now = DateTime.Now;
        static bool Missing(int? v) => v is null or 0;
        void Need(string key, int? v, string message) { if (Missing(v)) e.Add(key, message); }

        // Consumer
        e.Required("firstName", "First Name", m.FirstName, PatientFieldLimits.MaxNameLength);
        e.Required("lastName", "Last Name", m.LastName, PatientFieldLimits.MaxNameLength);
        e.Optional("providerPatientNo", "Provider Patient ID", m.ProviderPatientNo, PatientFieldLimits.MaxProviderPatientNoLength);
        if (!SsnPolicy.IsValid(m.Ssn)) e.Add("ssn", "Please provide the valid SSN!");
        if ((m.F2FAssessmentDateTime >= DobCutoff || m.CallEnded >= DobCutoff) && m.Dob is null) e.Add("dob", "Please provide the valid DOB!");
        if (m.Dob > DateTime.Today) e.Add("dob", "DOB cannot be future date!");

        // Crisis telephone
        if (wantsPhone)
        {
            if (m.CallEnded is null) e.Add("callEnded", "Call End Date is Required!");
            else
            {
                if (m.CallEnded > now) e.Add("callEnded", "Call End Date cannot be future date!");
                if (m.DispatchDateTime is { } d && m.CallEnded > d) e.Add("callEnded", "Call End Time must be before Dispatch Time");
            }
            if (m.DispatchDateTime > now) e.Add("dispatchDateTime", "Dispatch Date cannot be future date!");
            Need("dispositionId", m.DispositionId, "Disposition is required");
            if (m.DispositionId == OtherDisposition && string.IsNullOrWhiteSpace(m.DispositionOther)) e.Add("dispositionOther", "Detail must be provided when Disposition is 'Other'");
            e.Optional("dispositionOther", "Disposition, Other", m.DispositionOther, AssessmentFieldLimits.MaxDispositionOtherLength);
            if (m.DispositionId == MobileCrisisDispatched && m.DispatchDateTime is null) e.Add("dispatchDateTime", "Dispatch Date and Time are required for Mobile Crisis Staff.");
            if (m.DispositionId != MobileCrisisDispatched && m.DispatchDateTime is not null) e.Add("dispatchDateTime", "Dispatch Date should be empty unless Mobile Crisis Staff is selected.");
        }

        var accepted = m.Hospitalizations.Any(h => h.HospitalizationDispositionId == ReferralAccepted);

        if (isF2F)
        {
            Need("genderId", m.GenderId, "Please select the Gender!");
            Need("raceId", m.RaceId, "Please select the Race!");
            Need("ethnicityId", m.EthnicityId, "Please select the Ethnicity!");
            Need("assessmentTypeId", m.AssessmentTypeId, "Please select the Assessment Type!");

            if (m.F2FAssessmentDateTime is not { } seen) e.Add("f2FAssessmentDateTime", "Assessment Date is Required!");
            else
            {
                if (seen > now) e.Add("f2FAssessmentDateTime", "Assessment Date cannot be future date!");
                if (m.CallEnded is { } call && seen < call) e.Add("f2FAssessmentDateTime", "Assessment Date cannot be before Call End date!");
                if (m.DispatchDateTime is { } dispatch && seen <= dispatch) e.Add("f2FAssessmentDateTime", "Arrival date/time must be after dispatch date/time");
            }

            if (m.TransportedByLE is null) e.Add("transportedByLE", "Transported by Law Enforcement is required.");
            Need("payorSourceId", m.PayorSourceId, "Primary Insurer is required");
            if (m.AnnualHouseholdIncome < 0) e.Add("annualHouseholdIncome", "Please provide the valid Annual Gross Household Income");
            // These persist to byte columns; without a bound the cast silently wraps.
            if (m.NumberInHousehold is < 0 or > AssessmentFieldLimits.MaxByteColumnValue) e.Add("numberInHousehold", $"Number of People in Household must be between 0 and {AssessmentFieldLimits.MaxByteColumnValue}.");
            if (m.Arrests30Days is < 0 or > AssessmentFieldLimits.MaxByteColumnValue) e.Add("arrests30Days", $"Number of arrests in last 30 days must be between 0 and {AssessmentFieldLimits.MaxByteColumnValue}.");

            Need("assessmentLocationId", m.AssessmentLocationId, "Please select the Consumer Location at Assessment!");
            if (m.TelevideoAssessment is null) e.Add("televideoAssessment", "Assessment via Televideo is required.");
            Need("currentServicesId", m.CurrentServicesId, "Please select the Current Services Being Received!");
            Need("mhTreatmentDeclarationId", m.MHTreatmentDeclarationId, "Please select the Declaration of MH Treatment!");
            Need("motStatusId", m.MOTStatusId, "Please select the MOT Status!");
            Need("durablePOAId", m.DurablePOAId, "Please select the Durable POA!");
            Need("residentialStatusId", m.ResidentialStatusId, "Please select the Residential Status!");
            Need("countyId", m.CountyId, "Please select the County of Residence!");
            Need("employmentStatusId", m.EmploymentStatusId, "Please select the Employment Status!");
            Need("maritalStatusId", m.MaritalStatusId, "Please select the Marital Status!");
            Need("militaryStatusId", m.MilitaryStatusId, "Please select the Military Status!");
            Need("school3MonthsId", m.School3MonthsId, "Please select the Attended school in last 3 months!");
            Need("educationLevelId", m.EducationLevelId, "Please select the Current or highest grade completed!");
            Need("primaryProblemId", m.PrimaryProblemId, "Please select the Primary Problem that lead to Recommended Treatment!");
            Need("intellectualDisabilityId", m.IntellectualDisabilityId, "Please select the Intellectual / Development Disability!");
            Need("medicalInstabilityId", m.MedicalInstabilityId, "Please select the Medical / Physical Instability!");
            Need("medicationIssuesId", m.MedicationIssuesId, "Please select the Medication Compliance Issues!");
            Need("pastTraumaId", m.PastTraumaId, "Please select the Declaration of Past Trauma!");
            Need("substanceAbuseId", m.SubstanceAbuseId, "Please select the Substance Abuse!");

            if (m.SubstanceAbuseId == Yes && m.Drugs.Count == 0) e.Add("drugs", "At least one drug entry is required when Substance Abuse is selected.");
            if (m.Drugs.Any(d => Missing(d.DrugId))) e.Add("drugs", "Drug is required.");
            if (m.Drugs.Any(d => Missing(d.DrugRouteId))) e.Add("drugs", "Drug Route is required.");
            if (m.Drugs.Any(d => Missing(d.DrugFrequencyId))) e.Add("drugs", "Drug Frequency is required.");
            if (m.Drugs.Select(d => d.DrugId).Distinct().Count() != m.Drugs.Count) e.Add("drugs", "Drug value must be unique.");

            if (m.HospAlternatives.Count == 0) e.Add("hospAlternatives", "Alternative to Hospitalization is required.");
            if (m.HospAlternatives.Any(a => Missing(a.HospitalizationAlternativeId))) e.Add("hospAlternatives", "Hospitalization Alternative is required.");
            if (m.HospAlternatives.Any(a => Missing(a.HospAltDispositionListId))) e.Add("hospAlternatives", "Alt Disposition is required.");
            if (m.HospAlternatives.Select(a => a.HospitalizationAlternativeId).Distinct().Count() != m.HospAlternatives.Count) e.Add("hospAlternatives", "Alternatives to Hospitalization value must be unique.");

            if (m.Hospitalizations.Any(h => Missing(h.HospitalizationId))) e.Add("hospitalizations", "Referred To is required.");
            if (m.Hospitalizations.Any(h => Missing(h.HospitalizationDispositionId))) e.Add("hospitalizations", "Referred To Disposition is required.");
            if (m.Hospitalizations.Select(h => h.HospitalizationId).Distinct().Count() != m.Hospitalizations.Count) e.Add("hospitalizations", "Hospitalization value must be unique.");

            if (m.TimeDispositionCompleted is not { } done) e.Add("timeDispositionCompleted", "Date Disposition Completed is Required!");
            else
            {
                if (done > now) e.Add("timeDispositionCompleted", "Date Disposition Completed cannot be future date!");
                if (m.F2FAssessmentDateTime is { } arrived && done < arrived) e.Add("timeDispositionCompleted", "Time Disposition Completed must be after arrival time");
            }
            e.Required("completedByFirstName", "Assessment Completed By First Name", m.CompletedByFirstName, AssessmentFieldLimits.MaxCompletedByNameLength);
            e.Required("completedByLastName", "Assessment Completed By Last Name", m.CompletedByLastName, AssessmentFieldLimits.MaxCompletedByNameLength);

            // A referral that was accepted means the person was sent on: transport and admission details are then required.
            if (accepted)
            {
                if (m.TimeTransported is not { } sent) e.Add("timeTransported", "Date transported to receiving facility is Required!");
                else
                {
                    if (sent > now) e.Add("timeTransported", "Date transported to receiving facility cannot be future date!");
                    if (m.TimeDispositionCompleted is { } finished && sent < finished) e.Add("timeTransported", "Date transported to receiving facility cannot be before Date Disposition completed!");
                }
                Need("recommendedTransportModeId", m.RecommendedTransportModeId, "Please select the Recommended mode of Transport!");
                Need("firstHospitalizationId", m.FirstHospitalizationId, "Please select the 1st Hospitalization!");
                if (m.VoluntaryAdmissionRecommended is null) e.Add("voluntaryAdmissionRecommended", "Voluntary Admission Recommended is required!");
                if (m.FollowupContact == true && m.IsAdmitted is null) e.Add("isAdmitted", "Was the patient admitted is required!");
                if (m.TelehealthAdmissionAssessment is null) e.Add("telehealthAdmissionAssessment", "Admission assessment via telehealth is required!");
            }

            if (m.FollowupContact == true && !accepted && m.FollowupReportedServiceHelpful is null) e.Add("followupReportedServiceHelpful", "Report Service Helpful is required!");
            if (m.FollowupContact == false)
            {
                if (m.ContactAttempts is null) e.Add("contactAttempts", "No. of Attempts to contact is required!");
                else if (m.ContactAttempts <= 0) e.Add("contactAttempts", "Please provide the valid No. of Attempts to contact!");
            }
            if (m.ContactAttempts > AssessmentFieldLimits.MaxByteColumnValue) e.Add("contactAttempts", $"No. of Attempts to contact must be {AssessmentFieldLimits.MaxByteColumnValue} or less.");
        }
    }
}
