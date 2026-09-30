using CrisisManagement.Data;
using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.SP;
using CrisisManagement.Data.StoredProcedures;
using CrisisManagement.Features.Assessments.ViewModels;
using CrisisManagement.Infrastructure.Identity;
using CrisisManagement.Shared.Common;

namespace CrisisManagement.Features.Assessments.Services;

// Search Assessment. Runs the legacy usp_CMS_SearchAssessment_rd procedure (kept as a procedure on purpose). The caller
// is limited to their own providers: a chosen provider must be one of them, and the procedure also gets the list.
public sealed class AssessmentSearchService(IUnitOfWork uow, ProviderScope scope)
{
    private const int MaxText = 50;

    // Column names the procedure allows in ORDER BY (its outer select).
    private static readonly Dictionary<string, string> SortColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        ["assessmentType"] = "AssessmentType", ["assessmentDate"] = "AssessmentDate", ["lastName"] = "LastName",
        ["ssn"] = "SSN", ["dob"] = "DOB", ["providerName"] = "ProviderName"
    };

    public async Task<List<LookupItem>> GetProvidersAsync() =>
        (await uow.Providers.GetIdNamesAsync(scope.AllowedIds?.ToArray())).Select(p => new LookupItem(p.Id, p.Name, p.Short)).ToList();

    public async Task<PagedResult<AssessmentListItem>> SearchAsync(AssessmentSearchRequest r)
    {
        var errors = new ErrorBag();
        if (r.AssessmentDateFrom is { } from && r.AssessmentDateTo is { } to && from > to)
            errors.Add("assessmentDateTo", "Assessment Date(From) should be less than equal to Assessment Date(To).");
        errors.Optional("providerPatientNo", "Provider Patient ID", r.ProviderPatientNo, MaxText);
        errors.Optional("lastName", "Last Name", r.LastName, MaxText);
        errors.Optional("firstName", "First Name", r.FirstName, MaxText);
        errors.Optional("completedByLastName", "Assessment Completed By Last Name", r.CompletedByLastName, MaxText);
        errors.Optional("completedByFirstName", "Assessment Completed By First Name", r.CompletedByFirstName, MaxText);
        errors.Optional("providerF2FAssessmentId", "Provider Face to Face Assessment ID", r.ProviderF2FAssessmentId, MaxText);
        errors.Optional("providerPhoneAssessmentId", "Provider Phone Assessment ID", r.ProviderPhoneAssessmentId, MaxText);
        errors.ThrowIfAny();

        if (r.ProviderId is int chosen) scope.Require(chosen);
        var allowed = scope.AllowedIds;
        if (allowed is { Count: 0 }) return new();   // a user with no providers sees nothing

        var prefs = new SearchPreferences()
            .Add("ProviderID", r.ProviderId)
            .AddProviderIds("UserProviderIds", allowed)
            .Add("ProviderPatientNo", r.ProviderPatientNo)
            // An SSN that is not nine digits (or ddd-dd-dddd) is left out of the search, as in the Blazor CMS.
            .Add("SSN", SsnPolicy.TryNormalize(r.Ssn, out var ssn) ? ssn : null)
            .Add("BeginAssmntDate", r.AssessmentDateFrom)
            .Add("EndAssmntDate", r.AssessmentDateTo)
            .Add("LastName", r.LastName)
            .Add("FirstName", r.FirstName)
            .Add("AsmtLastName", r.CompletedByLastName)
            .Add("AsmtFirstName", r.CompletedByFirstName)
            .Add("F2FAssmntID", r.F2FAssessmentId)
            .Add("PhoneAssmntID", r.PhoneAssessmentId)
            .Add("ProviderF2FAssmntID", r.ProviderF2FAssessmentId)
            .Add("ProviderPhoneAssmntID", r.ProviderPhoneAssessmentId)
            .Add("TicklerList", r.IncompleteOnly ? "1" : null);

        var column = r.SortBy is not null && SortColumns.TryGetValue(r.SortBy, out var c) ? c : "AssessmentDate";
        var direction = r.SortBy is null || r.SortDesc ? "DESC" : "ASC";

        var (rows, total) = await uow.StoredProcedures.SearchAsync<UspAssessmentSearchRd>(
            StoredProcedureConstants.QualifiedAssessmentSearchRd, prefs, Math.Max(1, r.PageIndex), Math.Clamp(r.PageSize, 1, 200),
            $"{column} {direction}", x => x.TotalRowCount);

        return new PagedResult<AssessmentListItem>
        {
            TotalCount = total,
            Items = rows.Select(x => new AssessmentListItem(x.F2FAssessmentId, x.PhoneAssessmentId, x.PatientId, x.ProviderId, x.AssessmentDate,
                x.LastName, x.FirstName, x.SSN, x.DOB, x.ProviderPatientId, x.ProviderName, x.Abbreviation, x.AssessmentType)).ToList()
        };
    }
}
