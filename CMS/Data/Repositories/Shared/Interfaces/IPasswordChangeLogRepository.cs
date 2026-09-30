using CMS.Data.Models.Identity;
using CMS.Data.Repositories.Interfaces;

namespace CMS.Data.Repositories.Interfaces;

public interface IPasswordChangeLogRepository : IRepository<PasswordChangeLog>
{
    // Most recent password hashes, newest first.
    Task<List<string>> GetRecentHashesAsync(int userId, int take);
    // Tracked rows, for removal.
    Task<List<PasswordChangeLog>> GetForUserAsync(int userId);
}
