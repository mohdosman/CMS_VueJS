using CrisisManagement.Data.Context;
using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Data.Repositories.Interfaces;
using CrisisManagement.Features.Assessments.ViewModels;
using CrisisManagement.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace CrisisManagement.Data.Repositories;

public sealed class AssessmentRepository(AppDbContext context) : IAssessmentRepository
{
    public Task<F2FAssessment?> GetF2FAsync(int id) =>
        context.F2FAssessments
            .Include(a => a.Patient).Include(a => a.PhoneAssessment)
            .Include(a => a.F2FDrugs).Include(a => a.F2FHospitalizations)
            .Include(a => a.F2FHospAlternatives).ThenInclude(h => h.HospAltDisposition)
            .AsSplitQuery()
            .FirstOrDefaultAsync(a => a.F2FAssessmentId == id);

    public Task<PhoneAssessment?> GetPhoneAsync(int id) =>
        context.PhoneAssessments.Include(p => p.Patient).FirstOrDefaultAsync(p => p.PhoneAssessmentId == id);

    public Task<PhoneAssessment?> GetPhoneWithF2FsAsync(int id) =>
        context.PhoneAssessments
            .Include(p => p.F2FAssessments).ThenInclude(f => f.F2FDrugs)
            .Include(p => p.F2FAssessments).ThenInclude(f => f.F2FHospAlternatives)
            .Include(p => p.F2FAssessments).ThenInclude(f => f.F2FHospitalizations)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.PhoneAssessmentId == id);

    public Task<int?> GetLatestF2FIdAsync(int phoneId) =>
        context.F2FAssessments.AsNoTracking().Where(a => a.PhoneAssessmentId == phoneId)
            .OrderByDescending(a => a.F2FAssessmentId).Select(a => (int?)a.F2FAssessmentId).FirstOrDefaultAsync();

    public Task<bool> PhoneDuplicateExistsAsync(int excludePhoneId, int providerId, DateTime callEnded, string lastName) =>
        context.PhoneAssessments.AnyAsync(p => p.PhoneAssessmentId != excludePhoneId && p.ProviderId == providerId
            && p.CallEnded == callEnded && p.Patient.LastName == lastName);

    public Task<bool> F2FDuplicateExistsAsync(int excludeF2FId, int providerId, DateTime assessedOn, string lastName) =>
        context.F2FAssessments.AnyAsync(a => a.F2FAssessmentId != excludeF2FId && a.ProviderId == providerId
            && a.F2FAssessmentDate == assessedOn && a.Patient.LastName == lastName);

    public async Task<Dictionary<(int Alternative, int List), int>> GetHospAltDispositionMapAsync() =>
        (await context.HospAltDispositions.AsNoTracking()
            .Select(h => new { h.HospitalizationAlternativeId, h.HospAltDispositionListId, h.HospAltDispositionId }).ToListAsync())
        .ToDictionary(h => (h.HospitalizationAlternativeId, h.HospAltDispositionListId), h => h.HospAltDispositionId);

    public void AddPatient(Patient patient) => context.Patients.Add(patient);
    public void AddPhone(PhoneAssessment phone) => context.PhoneAssessments.Add(phone);
    public void AddF2F(F2FAssessment f2f) => context.F2FAssessments.Add(f2f);

    public void RemoveF2F(F2FAssessment f2f)
    {
        context.F2FDrugs.RemoveRange(f2f.F2FDrugs);
        context.F2FHospAlternatives.RemoveRange(f2f.F2FHospAlternatives);
        context.F2FHospitalizations.RemoveRange(f2f.F2FHospitalizations);
        context.F2FAssessments.Remove(f2f);
    }

    public void RemoveF2FChildren(F2FAssessment f2f)
    {
        context.F2FDrugs.RemoveRange(f2f.F2FDrugs);
        context.F2FHospAlternatives.RemoveRange(f2f.F2FHospAlternatives);
        context.F2FHospitalizations.RemoveRange(f2f.F2FHospitalizations);
        f2f.F2FDrugs.Clear();
        f2f.F2FHospAlternatives.Clear();
        f2f.F2FHospitalizations.Clear();
    }

    public void RemovePhone(PhoneAssessment phone) => context.PhoneAssessments.Remove(phone);

    // The same lists, orders and code/label columns the Blazor CMS used.
    public async Task<AssessmentLookups> GetLookupsAsync()
    {
        var c = context;
        return new AssessmentLookups
        {
            Genders = await c.Genders.AsNoTracking().OrderBy(x => x.GenderDescription).Select(x => new LookupItem(x.GenderId, x.GenderDescription, x.GenderDescription)).ToListAsync(),
            Races = await c.Races.AsNoTracking().OrderBy(x => x.RaceDescription).Select(x => new LookupItem(x.RaceId, x.RaceDescription, x.RaceDescription)).ToListAsync(),
            Ethnicities = await c.Ethnicities.AsNoTracking().OrderBy(x => x.EthnicityDescription).Select(x => new LookupItem(x.EthnicityId, x.EthnicityDescription, x.EthnicityDescription)).ToListAsync(),
            Dispositions = await c.Dispositions.AsNoTracking().OrderBy(x => x.DispositionDescription).Select(x => new LookupItem(x.DispositionId, x.DispositionDescription, x.DispositionCode)).ToListAsync(),
            AssessmentTypes = await c.AssessmentTypes.AsNoTracking().OrderBy(x => x.AssessmentTypeDescription).Select(x => new LookupItem(x.AssessmentTypeId, x.AssessmentTypeDescription, null)).ToListAsync(),
            AssessmentLocations = await c.AssessmentLocations.AsNoTracking().OrderBy(x => x.AssessmentLocationDescription).Select(x => new LookupItem(x.AssessmentLocationId, x.AssessmentLocationDescription, null)).ToListAsync(),
            PayorSources = await c.PayorSources.AsNoTracking().OrderBy(x => x.PayorSourceDescription).Select(x => new LookupItem(x.PayorSourceId, x.PayorSourceDescription, x.Abbreviation)).ToListAsync(),
            CurrentServices = await c.CurrentServices.AsNoTracking().OrderBy(x => x.CurrentServices).Select(x => new LookupItem(x.CurrentServicesId, x.CurrentServices, null)).ToListAsync(),
            YesNoUnknown = await c.YesNoUnknowns.AsNoTracking().OrderBy(x => x.YesNoUnknownId).Select(x => new LookupItem(x.YesNoUnknownId, x.Response, x.Abbreviation)).ToListAsync(),
            ResidentialStatuses = await c.ResidentialStatuses.AsNoTracking().OrderBy(x => x.ResidentialStatusDescription).Select(x => new LookupItem(x.ResidentialStatusId, x.ResidentialStatusDescription, null)).ToListAsync(),
            Counties = await c.Counties.AsNoTracking().OrderBy(x => x.CountyDescription).Select(x => new LookupItem(x.CountyId, x.CountyDescription, x.CountyCode)).ToListAsync(),
            EmploymentStatuses = await c.EmploymentStatuses.AsNoTracking().OrderBy(x => x.EmploymentStatusDescription).Select(x => new LookupItem(x.EmploymentStatusId, x.EmploymentStatusDescription, null)).ToListAsync(),
            MaritalStatuses = await c.MaritalStatuses.AsNoTracking().OrderBy(x => x.MaritalStatusDescription).Select(x => new LookupItem(x.MaritalStatusId, x.MaritalStatusDescription, x.MaritalStatusCode)).ToListAsync(),
            MilitaryStatuses = await c.MilitaryStatuses.AsNoTracking().OrderBy(x => x.MilitaryStatusDescription).Select(x => new LookupItem(x.MilitaryStatusId, x.MilitaryStatusDescription, x.MilitaryStatusCode)).ToListAsync(),
            EducationLevels = await c.EducationLevels.AsNoTracking().OrderBy(x => x.SortOrder).ThenBy(x => x.EducationLevelDescription).Select(x => new LookupItem(x.EducationLevelId, x.EducationLevelDescription, null)).ToListAsync(),
            PrimaryProblems = await c.PrimaryProblems.AsNoTracking().OrderBy(x => x.PrimaryProblemDescription).Select(x => new LookupItem(x.PrimaryProblemId, x.PrimaryProblemDescription, null)).ToListAsync(),
            Drugs = await c.Drugs.AsNoTracking().OrderBy(x => x.DrugDescription).Select(x => new LookupItem(x.DrugId, x.DrugDescription, null)).ToListAsync(),
            DrugRoutes = await c.DrugRoutes.AsNoTracking().OrderBy(x => x.DrugRouteDescription).Select(x => new LookupItem(x.DrugRouteId, x.DrugRouteDescription, null)).ToListAsync(),
            DrugFrequencies = await c.DrugFrequencies.AsNoTracking().OrderBy(x => x.DrugFrequencyDescription).Select(x => new LookupItem(x.DrugFrequencyId, x.DrugFrequencyDescription, null)).ToListAsync(),
            HospitalizationAlternatives = await c.HospitalizationAlternatives.AsNoTracking().OrderBy(x => x.Hierarchy).ThenBy(x => x.HospitalizationAlternativeDescription)
                .Select(x => new LookupItem(x.HospitalizationAlternativeId, x.HospitalizationAlternativeDescription, x.Abbreviation)).ToListAsync(),
            HospAltDispositions = await c.HospAltDispositions.AsNoTracking().OrderBy(x => x.HospitalizationAlternativeId).ThenBy(x => x.HospAltDispositionListId)
                .Select(x => new HospAltDispositionLookup(x.HospitalizationAlternativeId, x.HospAltDispositionListId, x.HospAltDispositionList.HospAltDisposition)).ToListAsync(),
            Hospitalizations = await c.Hospitalizations.AsNoTracking().OrderBy(x => x.HospitalizationDescription).Select(x => new LookupItem(x.HospitalizationId, x.HospitalizationDescription, x.Abbreviation)).ToListAsync(),
            HospitalizationDispositions = await c.HospitalizationDispositions.AsNoTracking().OrderBy(x => x.HospitalizationDispositionDescription)
                .Select(x => new LookupItem(x.HospitalizationDispositionId, x.HospitalizationDispositionDescription, x.Abbreviation)).ToListAsync(),
            TransportModes = await c.RecommendedTransportModes.AsNoTracking().OrderBy(x => x.RecommendedTransportModeDescription)
                .Select(x => new LookupItem(x.RecommendedTransportModeId, x.RecommendedTransportModeDescription, x.Abbreviation)).ToListAsync()
        };
    }
}
