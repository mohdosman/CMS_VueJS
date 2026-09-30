using CMS.Data.Models.Identity;
using CMS.Data.Repositories.Interfaces;

namespace CMS.Data.Repositories.Interfaces;

public interface IUserRoleRepository : IRepository<ApplicationUserRole>
{
    // Roles a user holds, ordered by name.
    Task<List<IdName>> GetRolesForUserAsync(int userId);
    // Tracked rows, for removal.
    Task<List<ApplicationUserRole>> GetForUserAsync(int userId);
}
