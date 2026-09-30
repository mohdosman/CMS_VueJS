using CMS.Data.Models.Domain;
using CMS.Data.Repositories.Interfaces;

namespace CMS.Data.Repositories.Interfaces;

public interface IProviderUserRepository : IRepository<ProviderUser>
{
    Task<List<int>> GetProviderIdsForUserAsync(int userId);
    Task<List<IdName>> GetProvidersForUserAsync(int userId);
    // Tracked rows, for removal.
    Task<List<ProviderUser>> GetForUserAsync(int userId);
}
