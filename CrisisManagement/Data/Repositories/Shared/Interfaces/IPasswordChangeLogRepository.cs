using CrisisManagement.Data.Models.Identity;
using CrisisManagement.Data.Repositories.Interfaces;

namespace CrisisManagement.Data.Repositories.Interfaces;

public interface IPasswordChangeLogRepository : IRepository<PasswordChangeLog>
{
    // Most recent password hashes, newest first.
    Task<List<string>> GetRecentHashesAsync(int userId, int take);
    // Tracked rows, for removal.
    Task<List<PasswordChangeLog>> GetForUserAsync(int userId);
}
