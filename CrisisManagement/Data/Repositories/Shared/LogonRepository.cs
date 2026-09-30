using CrisisManagement.Data.Context;
using CrisisManagement.Data.Models.Identity;
using CrisisManagement.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CrisisManagement.Data.Repositories;

public sealed class LogonRepository(AppDbContext context) : Repository<Logon>(context), ILogonRepository
{
    public Task<DateTime?> GetLastLogOnAsync(int userId) =>
        _entities.AsNoTracking().Where(l => l.UserId == userId).Select(l => (DateTime?)l.LogOnDateTime).MaxAsync();

    // Set-based delete: an active user can have many rows.
    public Task<int> DeleteForUserAsync(int userId) => _entities.Where(l => l.UserId == userId).ExecuteDeleteAsync();
}
