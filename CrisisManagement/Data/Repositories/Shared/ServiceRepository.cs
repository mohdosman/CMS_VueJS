using CrisisManagement.Data.Context;
using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Data.Repositories.Interfaces;
using CrisisManagement.Features.Services.ViewModels;
using CrisisManagement.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace CrisisManagement.Data.Repositories;

public sealed class ServiceRepository(AppDbContext context) : Repository<Service>(context), IServiceRepository
{
    public Task<Service?> GetAggregateAsync(int id) =>
        context.Services.Include(s => s.Patient).FirstOrDefaultAsync(s => s.ServiceId == id);

    public Task<bool> DuplicateExistsAsync(int excludeServiceId, int patientId, int serviceCodeId, DateTime dosAdmitDate) =>
        context.Services.AsNoTracking().AnyAsync(s => s.PatientId == patientId && s.ServiceCodeId == serviceCodeId
            && s.DOSAdmitDate == dosAdmitDate && s.ServiceId != excludeServiceId);

    // A patient belongs to a provider through a service or an assessment of that provider.
    private IQueryable<Patient> PatientsOfProvider(int providerId, string providerPatientNo) =>
        context.Patients.Where(p => p.ProviderPatientNo == providerPatientNo &&
            (context.Services.Any(s => s.PatientId == p.PatientId && s.ProviderId == providerId) ||
             context.F2FAssessments.Any(f => f.PatientId == p.PatientId && f.ProviderId == providerId) ||
             context.PhoneAssessments.Any(ph => ph.PatientId == p.PatientId && ph.ProviderId == providerId)));

    public async Task<List<Patient>> FindExistingPatientsAsync(int providerId, string providerPatientNo)
    {
        var newest = PatientsOfProvider(providerId, providerPatientNo).GroupBy(p => new { p.LastName, p.FirstName, p.DOB }).Select(g => g.Max(p => p.PatientId));
        return await context.Patients.AsNoTracking().Where(p => newest.Contains(p.PatientId)).OrderBy(p => p.PatientId).ToListAsync();
    }

    public Task<Patient?> FindPatientAsync(int providerId, string providerPatientNo, string lastName, string? ssn, DateTime? dob)
    {
        ssn ??= "";
        return PatientsOfProvider(providerId, providerPatientNo)
            .Where(p => p.LastName == lastName && (p.SSN ?? "") == ssn && p.DOB == dob).OrderByDescending(p => p.PatientId).FirstOrDefaultAsync();
    }

    public Task<Patient?> GetPatientAsync(int id) => context.Patients.FirstOrDefaultAsync(p => p.PatientId == id);

    public async Task<ServiceLookups> GetLookupsAsync() => new()
    {
        Genders = await context.Genders.AsNoTracking().OrderBy(g => g.GenderId).Select(g => new LookupItem(g.GenderId, g.GenderDescription, null)).ToListAsync(),
        Counties = await context.Counties.AsNoTracking().OrderBy(c => c.CountyDescription).Select(c => new LookupItem(c.CountyId, c.CountyDescription, c.CountyCode)).ToListAsync(),
        PayorSources = await context.PayorSources.AsNoTracking().OrderBy(s => s.PayorSourceDescription).Select(s => new LookupItem(s.PayorSourceId, s.PayorSourceDescription, s.Abbreviation)).ToListAsync(),
        ServiceCodes = await context.ServiceCodes.AsNoTracking().OrderBy(s => s.ServiceCodeId).Select(s => new LookupItem(s.ServiceCodeId, s.ServiceCodeName, s.ServiceCodeAbbrev)).ToListAsync(),
        ServiceCodeRules = await context.ServiceCodes.AsNoTracking().Select(s => new ServiceCodeRule(s.ServiceCodeId, s.IsDurationHoursRequired, s.IsDischargeDateRequired)).ToListAsync()
    };
}
