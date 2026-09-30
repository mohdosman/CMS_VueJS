using CrisisManagement.Data.Models.Identity;
using CrisisManagement.Data.Repositories.Interfaces;

namespace CrisisManagement.Data.Repositories.Interfaces;

public interface IUserRoleRepository : IRepository<ApplicationUserRole>
{
    // Roles a user holds, ordered by name.
    Task<List<IdName>> GetRolesForUserAsync(int userId);
    Task<int> CountForRoleAsync(int roleId);
    // Tracked rows, for removal.
    Task<List<ApplicationUserRole>> GetForUserAsync(int userId);
}
