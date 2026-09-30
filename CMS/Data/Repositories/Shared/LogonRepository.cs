using CMS.Data.Context;
using CMS.Data.Models.Identity;
using CMS.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CMS.Data.Repositories;

public sealed class LogonRepository(AppDbContext context) : Repository<Logon>(context), ILogonRepository
{
    public Task<DateTime?> GetLastLogOnAsync(int userId) =>
        _entities.AsNoTracking().Where(l => l.UserId == userId).Select(l => (DateTime?)l.LogOnDateTime).MaxAsync();

    // Set-based delete: an active user can have many rows.
    public Task<int> DeleteForUserAsync(int userId) => _entities.Where(l => l.UserId == userId).ExecuteDeleteAsync();
}
