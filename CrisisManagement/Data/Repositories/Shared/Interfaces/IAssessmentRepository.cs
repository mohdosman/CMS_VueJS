using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Features.Assessments.ViewModels;

namespace CrisisManagement.Data.Repositories.Interfaces;

// The phone and face to face assessments with their patient and child rows (Enter/Edit Assessment).
public interface IAssessmentRepository
{
    // Tracked aggregates; null when there is no such row.
    Task<F2FAssessment?> GetF2FAsync(int id);
    Task<PhoneAssessment?> GetPhoneAsync(int id);
    Task<PhoneAssessment?> GetPhoneWithF2FsAsync(int id);   // for delete: with every F2F and its child rows

    // The newest F2F of a phone assessment (a phone call that already produced one is edited as that F2F).
    Task<int?> GetLatestF2FIdAsync(int phoneId);

    Task<bool> PhoneDuplicateExistsAsync(int excludePhoneId, int providerId, DateTime callEnded, string lastName);
    Task<bool> F2FDuplicateExistsAsync(int excludeF2FId, int providerId, DateTime assessedOn, string lastName);

    // (alternative id, disposition list id) -> HospAltDispositionId.
    Task<Dictionary<(int Alternative, int List), int>> GetHospAltDispositionMapAsync();

    void AddPatient(Patient patient);
    void AddPhone(PhoneAssessment phone);
    void AddF2F(F2FAssessment f2f);

    // A F2F together with its child rows.
    void RemoveF2F(F2FAssessment f2f);
    // Empties the child rows of a F2F that is being saved (they are re-added from the request).
    void RemoveF2FChildren(F2FAssessment f2f);
    void RemovePhone(PhoneAssessment phone);

    Task<AssessmentLookups> GetLookupsAsync();
}
