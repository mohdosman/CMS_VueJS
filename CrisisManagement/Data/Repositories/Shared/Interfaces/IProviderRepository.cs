using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Data.Repositories.Interfaces;

namespace CrisisManagement.Data.Repositories.Interfaces;

public interface IProviderRepository : IRepository<Provider>
{
    // All providers ordered by name, or only the given ids when onlyIds is not null.
    Task<List<IdName>> GetIdNamesAsync(IReadOnlyCollection<int>? onlyIds = null);
}
