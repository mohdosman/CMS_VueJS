using CMS.Data.Models.Domain;
using CMS.Data.Repositories.Interfaces;

namespace CMS.Data.Repositories.Interfaces;

public interface IProviderRepository : IRepository<Provider>
{
    // All providers ordered by name, or only the given ids when onlyIds is not null.
    Task<List<IdName>> GetIdNamesAsync(IReadOnlyCollection<int>? onlyIds = null);
}
