using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Data.Repositories.Interfaces;
using CrisisManagement.Features.Providers.ViewModels;
using CrisisManagement.Shared.Common;

namespace CrisisManagement.Data.Repositories.Interfaces;

public interface IProviderRepository : IRepository<Provider>
{
    // All providers ordered by name, or only the given ids when onlyIds is not null.
    Task<List<IdName>> GetIdNamesAsync(IReadOnlyCollection<int>? onlyIds = null);

    // Providers screen. onlyIds null = unrestricted (administrator), otherwise only those provider ids.
    Task<PagedResult<ProviderListItem>> SearchAsync(ProviderSearchRequest request, IReadOnlyCollection<int>? onlyIds);

    // Tracked, with both addresses and their contacts, for reading and updating.
    Task<Provider?> GetAggregateAsync(int id);

    Task<bool> NpiExistsAsync(string npi, int exceptId);
    Task<bool> EdisonExistsAsync(string edisonNumber, int exceptId);

    // True when users, contracts, assessments or services still point at the provider.
    Task<bool> HasReferencesAsync(int id);

    // Id, name and abbreviation of the provider with this NPI; null when none.
    Task<IdName?> GetIdNameByNpiAsync(string npi);

    Task<List<LookupItem>> GetStatesAsync();
    Task<List<LookupItem>> GetCountiesAsync();

    // Removes a provider address together with the address and contact rows it owns.
    void RemoveAddressGraph(ProviderAddress providerAddress);
    void RemoveContact(Contact contact);
}
