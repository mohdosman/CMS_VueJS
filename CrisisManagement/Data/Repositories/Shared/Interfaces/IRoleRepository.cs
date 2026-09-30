using CrisisManagement.Data.Models.Identity;
using CrisisManagement.Data.Repositories.Interfaces;

namespace CrisisManagement.Data.Repositories.Interfaces;

public interface IRoleRepository : IRepository<ApplicationRole>
{
    // All roles ordered by name, or only the given ids when onlyIds is not null.
    Task<List<IdName>> GetIdNamesAsync(IReadOnlyCollection<int>? onlyIds = null);

    // Page of roles whose name contains nameLike (null = all); sortBy is "id" or "name" (default).
    Task<(List<IdName> Items, int Total)> SearchAsync(string? nameLike, string? sortBy, bool desc, int page, int size);

    // Tracked, for updates.
    Task<ApplicationRole?> GetByIdAsync(int id);
    Task<bool> NameExistsAsync(string normalizedName, int exceptId);
}
