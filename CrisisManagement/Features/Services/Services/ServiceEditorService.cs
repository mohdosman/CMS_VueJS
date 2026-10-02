using CrisisManagement.Data;
using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Features.Services.ViewModels;
using CrisisManagement.Infrastructure.Identity;
using CrisisManagement.Shared.Common;
using CrisisManagement.Shared.Constants;
using CrisisManagement.Shared.Extensions;
using Microsoft.EntityFrameworkCore;

namespace CrisisManagement.Features.Services.Services;

// Enter/Edit Service: one service (a patient's stay or visit at a provider) and, when it is created, the patient it belongs
// to. Everything is limited to the caller's providers.
public sealed class ServiceEditorService(IUnitOfWork uow, ProviderScope scope, CurrentSession session, IHttpContextAccessor http)
{
    public Task<ServiceLookups> GetLookupsAsync() => uow.Services.GetLookupsAsync();

    // Null = no such service.
    public async Task<ServiceEditModel?> GetAsync(int id)
    {
        var s = await uow.Services.GetAggregateAsync(id);
        if (s is null) return null;
        scope.Require(s.ProviderId);
        return ToModel(s);
    }

    // The patients the provider already has under this provider patient number (none, one, or several that differ).
    public async Task<List<ExistingPatient>> FindExistingPatientsAsync(int providerId, string? providerPatientNo)
    {
        scope.Require(providerId);
        var no = providerPatientNo?.Trim() ?? "";
        if (no.Length == 0) return [];
        return (await uow.Services.FindExistingPatientsAsync(providerId, no))
            .Select(p => new ExistingPatient(p.PatientId, p.ProviderPatientNo, p.SSN, p.FirstName, p.LastName, p.DOB, p.GenderId)).ToList();
    }

    // ---------------------------------------------------------------- save

    public async Task<ServiceEditModel> CreateAsync(ServiceEditModel m)
    {
        var errors = new ErrorBag();
        if (m.ProviderId is not > 0) errors.Add("providerId", "Please select the Provider!");
        errors.ThrowIfAny();
        scope.Require(m.ProviderId!.Value);

        var rules = (await uow.Services.GetLookupsAsync()).ServiceCodeRules;
        ValidateService(m, rules, errors);
        ValidatePatient(m, errors);
        errors.ThrowIfAny();

        var providerId = m.ProviderId!.Value;
        var ppn = m.ProviderPatientNo!.Trim();
        var patient = await ResolvePatientAsync(m, providerId, ppn, errors);
        errors.ThrowIfAny();

        var s = new Service { ProviderId = providerId, Patient = patient };
        Apply(s, m);
        if (patient.PatientId != 0 && await uow.Services.DuplicateExistsAsync(0, patient.PatientId, s.ServiceCodeId, s.DOSAdmitDate))
            throw ValidationFailedException.For("dosAdmitDate", "Duplicate Service.");

        uow.Services.Add(s);
        await SaveAsync();
        return ToModel((await uow.Services.GetAggregateAsync(s.ServiceId))!);
    }

    // Null = no such service. The patient of an existing service is not edited here (the form shows it read-only).
    public async Task<ServiceEditModel?> UpdateAsync(int id, ServiceEditModel m)
    {
        var s = await uow.Services.GetAggregateAsync(id);
        if (s is null) return null;
        scope.Require(s.ProviderId, "You do not have access to this service.");
        var errors = new ErrorBag();
        if (m.ProviderId is not > 0) errors.Add("providerId", "Please select the Provider!");
        errors.ThrowIfAny();
        scope.Require(m.ProviderId!.Value, "You do not have access to this service.");

        ValidateService(m, (await uow.Services.GetLookupsAsync()).ServiceCodeRules, errors);
        errors.ThrowIfAny();

        // Compared here rather than through EF's original value, which a reload would overwrite.
        if (!string.IsNullOrEmpty(m.RowVersion) && !s.Version.AsSpan().SequenceEqual(Convert.FromBase64String(m.RowVersion)))
            throw new ConflictException("This service was changed by someone else. Reload the page and try again.");

        s.ProviderId = m.ProviderId.Value;
        Apply(s, m);
        if (await uow.Services.DuplicateExistsAsync(id, s.PatientId, s.ServiceCodeId, s.DOSAdmitDate))
            throw ValidationFailedException.For("dosAdmitDate", "Duplicate Service.");

        await SaveAsync();
        return ToModel((await uow.Services.GetAggregateAsync(id))!);
    }

    // False = no such service. The legacy audit trigger reads who deleted from CMS_TempTrigger, keyed by the connection,
    // so the procedure and the delete run on one connection inside a transaction.
    public async Task<bool> DeleteAsync(int id)
    {
        var s = await uow.Services.GetAggregateAsync(id);
        if (s is null) return false;
        scope.Require(s.ProviderId, "You do not have access to this service.");

        await using var tx = await uow.BeginTransactionAsync();
        await uow.StoredProcedures.ExecuteAsync("EXEC dbo.usp_CMS_TempTrigger_upd @i_UserAltered = {0}", http.HttpContext!.User.GetUserId());
        uow.Services.Remove(s);
        await SaveAsync();
        await tx.CommitAsync();
        return true;
    }

    private async Task SaveAsync()
    {
        try { await uow.SaveChangesAsync(); }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase) == true)
        {
            throw ValidationFailedException.For("form", "One of the selected values is not valid. Reload the page and try again.");
        }
    }

    // ---------------------------------------------------------------- patient

    // An existing patient chosen on the form has its first name and gender updated; otherwise the provider's matching patient
    // is reused, and only when there is none is a new one created.
    private async Task<Patient> ResolvePatientAsync(ServiceEditModel m, int providerId, string ppn, ErrorBag errors)
    {
        var firstName = m.FirstName!.Trim();
        if (m.PatientId is > 0)
        {
            var known = await uow.Services.FindExistingPatientsAsync(providerId, ppn);
            var patient = known.Any(p => p.PatientId == m.PatientId) ? await uow.Services.GetPatientAsync(m.PatientId.Value) : null;
            if (patient is null) { errors.Add("patientId", "The selected patient is not valid."); return new(); }
            patient.FirstName = firstName;
            patient.GenderId = (byte)m.GenderId!.Value;
            return patient;
        }

        var ssn = SsnPolicy.TryNormalize(m.Ssn, out var digits) ? digits : null;
        var lastName = m.LastName!.Trim();
        var dob = m.Dob!.Value.Date;
        var same = await uow.Services.FindPatientAsync(providerId, ppn, lastName, ssn, dob);
        if (same is not null)
        {
            same.FirstName = firstName;
            same.GenderId = (byte)m.GenderId!.Value;
            return same;
        }
        return new Patient { ProviderPatientNo = ppn, SSN = ssn, LastName = lastName, FirstName = firstName, DOB = dob, GenderId = (byte)m.GenderId!.Value };
    }

    // ---------------------------------------------------------------- mapping

    private void Apply(Service s, ServiceEditModel m)
    {
        s.CountyId = (short)m.CountyId!.Value;
        s.ServiceCodeId = m.ServiceCodeId!.Value;
        s.PayorSourceId = m.PayorSourceId!.Value;
        s.PrimaryInsurerId = m.PrimaryInsurerId is > 0 ? m.PrimaryInsurerId : null;
        s.DOSAdmitDate = m.DosAdmitDate!.Value.Date;
        s.DischargeDate = m.DischargeDate?.Date;
        s.DurationHours = m.DurationHours;
        s.ServiceCountyId = (short)m.ServiceCountyId!.Value;
        s.SessionId = session.Id;
    }

    private static ServiceEditModel ToModel(Service s) => new()
    {
        Id = s.ServiceId, RowVersion = Convert.ToBase64String(s.Version), ProviderId = s.ProviderId,
        PatientId = s.PatientId, ProviderPatientNo = s.Patient.ProviderPatientNo, Ssn = s.Patient.SSN, FirstName = s.Patient.FirstName,
        LastName = s.Patient.LastName, Dob = s.Patient.DOB, GenderId = s.Patient.GenderId, CountyId = s.CountyId,
        PayorSourceId = s.PayorSourceId, PrimaryInsurerId = s.PrimaryInsurerId, ServiceCodeId = s.ServiceCodeId, DosAdmitDate = s.DOSAdmitDate,
        DischargeDate = s.DischargeDate, DurationHours = s.DurationHours, ServiceCountyId = s.ServiceCountyId
    };

    // ---------------------------------------------------------------- validation (the Blazor CMS rules and messages)

    private static void ValidateService(ServiceEditModel m, List<ServiceCodeRule> rules, ErrorBag e)
    {
        if (m.CountyId is not > 0) e.Add("countyId", "Please select the County of Residence.");
        if (m.PayorSourceId is not > 0) e.Add("payorSourceId", "Please select the Payor Billed for Service.");
        if (m.ServiceCountyId is not > 0) e.Add("serviceCountyId", "Please select the County of Service.");
        if (m.CountyId > short.MaxValue || m.ServiceCountyId > short.MaxValue) e.Add("form", "One of the selected values is not valid.");

        if (m.DosAdmitDate is not { } admit) e.Add("dosAdmitDate", "DOS or Admit Date is Required.");
        else if (admit.Date > DateTime.Today) e.Add("dosAdmitDate", "DOS or Admit Date cannot be future date!");
        if (m.DischargeDate is { } discharge)
        {
            if (m.DosAdmitDate is { } a && discharge.Date < a.Date) e.Add("dischargeDate", "Discharge Date cannot be before DOS Admit date!");
            if (discharge.Date > DateTime.Today) e.Add("dischargeDate", "Discharge Date cannot be future date!");
        }
        if (m.DurationHours is { } hours && hours is < 1 or > 999) e.Add("durationHours", "Please Enter Valid Duration (Hours) greater than 0 upto 3 digits.");

        // Discharge date and duration are required per service code; an unknown code keeps both required.
        var rule = rules.FirstOrDefault(r => r.ServiceCodeId == m.ServiceCodeId);
        if (m.ServiceCodeId is not > 0) e.Add("serviceCodeId", "Please select the Service.");
        else if (rule is null) e.Add("serviceCodeId", "Invalid ServiceCode.");
        if ((rule?.IsDischargeDateRequired ?? true) && m.DischargeDate is null) e.Add("dischargeDate", "Discharge Date is Required.");
        if ((rule?.IsDurationHoursRequired ?? true) && m.DurationHours is null) e.Add("durationHours", "Duration (Hours) is Required.");
    }

    private static void ValidatePatient(ServiceEditModel m, ErrorBag e)
    {
        var ppn = m.ProviderPatientNo?.Trim() ?? "";
        if (ppn.Length == 0) e.Add("providerPatientNo", "Provider Patient No is Required.");
        else if (ppn.Length > PatientFieldLimits.MaxProviderPatientNoLength) e.Add("providerPatientNo", $"Provider patient number cannot exceed {PatientFieldLimits.MaxProviderPatientNoLength} characters.");
        var first = m.FirstName?.Trim() ?? "";
        if (first.Length == 0) e.Add("firstName", "First Name is Required.");
        else if (first.Length > PatientFieldLimits.MaxNameLength) e.Add("firstName", $"First name cannot exceed {PatientFieldLimits.MaxNameLength} characters.");
        var last = m.LastName?.Trim() ?? "";
        if (last.Length == 0) e.Add("lastName", "Last Name is Required.");
        else if (last.Length > PatientFieldLimits.MaxNameLength) e.Add("lastName", $"Last name cannot exceed {PatientFieldLimits.MaxNameLength} characters.");
        if (m.Dob is not { } dob) e.Add("dob", "DOB is Required.");
        else if (dob.Date > DateTime.Today) e.Add("dob", "DOB cannot be future date!");
        if (m.GenderId is not > 0) e.Add("genderId", "Please select the Gender!");
        else if (m.GenderId > byte.MaxValue) e.Add("form", "One of the selected values is not valid.");
        if (!SsnPolicy.IsValid(m.Ssn)) e.Add("ssn", "SSN format is not valid.");
    }
}
