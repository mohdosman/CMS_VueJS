using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Data.Repositories.Interfaces;

namespace CrisisManagement.Data.Repositories.Interfaces;

public interface IProviderUserRepository : IRepository<ProviderUser>
{
    Task<List<int>> GetProviderIdsForUserAsync(int userId);
    Task<List<IdName>> GetProvidersForUserAsync(int userId);
    // Tracked rows, for removal.
    Task<List<ProviderUser>> GetForUserAsync(int userId);
}
