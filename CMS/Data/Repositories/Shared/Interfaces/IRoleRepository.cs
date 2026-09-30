using CMS.Data.Models.Identity;
using CMS.Data.Repositories.Interfaces;

namespace CMS.Data.Repositories.Interfaces;

public interface IRoleRepository : IRepository<ApplicationRole>
{
    // All roles ordered by name, or only the given ids when onlyIds is not null.
    Task<List<IdName>> GetIdNamesAsync(IReadOnlyCollection<int>? onlyIds = null);
}
