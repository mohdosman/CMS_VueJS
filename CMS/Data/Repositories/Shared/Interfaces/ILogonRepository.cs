using CMS.Data.Models.Identity;
using CMS.Data.Repositories.Interfaces;

namespace CMS.Data.Repositories.Interfaces;

public interface ILogonRepository : IRepository<Logon>
{
    Task<DateTime?> GetLastLogOnAsync(int userId);
    Task<int> DeleteForUserAsync(int userId);
}
