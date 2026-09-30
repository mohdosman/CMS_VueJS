using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Features.Services.ViewModels;

namespace CrisisManagement.Data.Repositories.Interfaces;

// Services (CMS_Service) and the lookups and patient matching the Enter Service screen needs.
public interface IServiceRepository : IRepository<Service>
{
    // Tracked, with the patient.
    Task<Service?> GetAggregateAsync(int id);

    // Another service of the same patient with the same service code and admit date.
    Task<bool> DuplicateExistsAsync(int excludeServiceId, int patientId, int serviceCodeId, DateTime dosAdmitDate);

    // Patients of the provider that carry this provider patient number, one per name and birth date (the newest).
    Task<List<Patient>> FindExistingPatientsAsync(int providerId, string providerPatientNo);

    // The tracked patient the provider already has with exactly these details; null when none.
    Task<Patient?> FindPatientAsync(int providerId, string providerPatientNo, string lastName, string? ssn, DateTime? dob);

    Task<Patient?> GetPatientAsync(int id);

    Task<ServiceLookups> GetLookupsAsync();
}
