using System.Text.RegularExpressions;
using CrisisManagement.Data;
using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.SP;
using CrisisManagement.Data.StoredProcedures;
using CrisisManagement.Features.Services.ViewModels;
using CrisisManagement.Infrastructure.Identity;
using CrisisManagement.Shared.Common;

namespace CrisisManagement.Features.Services.Services;

// Manage Service search. Runs the legacy usp_CMS_ServiceSearch_rd procedure, which builds its query by string
// concatenation: text filters are quote-doubled and checked with TextSafety, ids are integers, dates are formatted here.
// The caller is limited to their own providers.
public sealed class ServiceSearchService(IUnitOfWork uow, ProviderScope scope, CurrentSession session)
{
    private static readonly Regex SsnDigits = new(@"^\d{9}$");

    private static readonly Dictionary<string, string> SortColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        ["serviceId"] = "ServiceId", ["firstName"] = "FirstName", ["lastName"] = "LastName", ["providerPatientNo"] = "ProviderPatientNo",
        ["providerAbbrev"] = "ProviderAbbrev", ["serviceCodeAbbrev"] = "ServiceCodeAbbrev", ["ssn"] = "SSN",
        ["dosAdmitDate"] = "DOSAdmitDate", ["dischargeDate"] = "DischargeDate"
    };

    public async Task<List<LookupItem>> GetProvidersAsync() =>
        (await uow.Providers.GetIdNamesAsync(scope.AllowedIds?.ToArray())).Select(p => new LookupItem(p.Id, p.Name, p.Short)).ToList();

    public async Task<List<LookupItem>> GetServiceCodesAsync() => (await uow.Services.GetLookupsAsync()).ServiceCodes;

    // currentSessionOnly: only the services entered in the caller's current session (the grid under the Enter Service form).
    public async Task<PagedResult<ServiceListItem>> SearchAsync(ServiceSearchRequest r, bool currentSessionOnly = false)
    {
        var errors = new ErrorBag();
        if (r.DosAdmitDateFrom is { } from && r.DosAdmitDateTo is { } to && from > to)
            errors.Add("dosAdmitDateTo", "DOS/Admit Date(From) should be less than equal to DOS/Admit Date(To).");
        if (!string.IsNullOrWhiteSpace(r.Ssn) && !SsnDigits.IsMatch(r.Ssn.Trim())) errors.Add("ssn", "SSN is 9 digits, no hyphens or spaces.");
        foreach (var (key, label, value) in new[]
        {
            ("providerPatientNo", "Provider Patient ID", r.ProviderPatientNo), ("lastName", "Last Name", r.LastName), ("firstName", "First Name", r.FirstName)
        })
            if (TextSafety.Problem(value, label) is { } problem) errors.Add(key, problem);
        errors.ThrowIfAny();

        if (r.ProviderId is int chosen) scope.Require(chosen);
        var allowed = scope.AllowedIds;
        if (allowed is { Count: 0 }) return new();   // a user with no providers sees nothing

        var prefs = new SearchPreferences()
            .Add("ProviderID", r.ProviderId)
            .AddProviderIds("UserProviderIds", allowed)
            .Add("ServiceCodeId", r.ServiceCodeId)
            .AddEscaped("ProviderPatientNo", r.ProviderPatientNo)
            .Add("SSN", string.IsNullOrWhiteSpace(r.Ssn) ? null : r.Ssn.Trim())
            .Add("DOSAdmitDateFrom", r.DosAdmitDateFrom)
            .Add("DOSAdmitDateTo", r.DosAdmitDateTo)
            .AddEscaped("LastName", r.LastName)
            .AddEscaped("FirstName", r.FirstName)
            .Add("ServiceFileId", r.ServiceFileId);
        if (currentSessionOnly) prefs.Add("SessionId", session.Id);

        var column = r.SortBy is not null && SortColumns.TryGetValue(r.SortBy, out var c) ? c : "DOSAdmitDate";
        var direction = r.SortBy is null || r.SortDesc ? "DESC" : "ASC";

        var (rows, total) = await uow.StoredProcedures.SearchAsync<UspServiceSearchRd>(
            StoredProcedureConstants.QualifiedServiceSearchRd, prefs, Math.Max(1, r.PageIndex), Math.Clamp(r.PageSize, 1, 200),
            $"{column} {direction}", x => x.TotalRowCount);

        return new PagedResult<ServiceListItem>
        {
            TotalCount = total,
            Items = rows.Select(x => new ServiceListItem(x.ServiceId, x.ProviderPatientNo, x.SSN, x.LastName, x.FirstName, x.DOB,
                x.ProviderAbbrev, x.ServiceCodeAbbrev, x.DOSAdmitDate, x.DischargeDate)).ToList()
        };
    }
}
