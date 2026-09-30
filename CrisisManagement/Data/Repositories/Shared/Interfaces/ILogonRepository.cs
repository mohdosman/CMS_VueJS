using CrisisManagement.Data.Models.Identity;
using CrisisManagement.Data.Repositories.Interfaces;

namespace CrisisManagement.Data.Repositories.Interfaces;

public interface ILogonRepository : IRepository<Logon>
{
    Task<DateTime?> GetLastLogOnAsync(int userId);
    Task<int> DeleteForUserAsync(int userId);
}
