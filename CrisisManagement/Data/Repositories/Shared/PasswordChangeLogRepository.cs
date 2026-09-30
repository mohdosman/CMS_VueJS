using CrisisManagement.Data.Context;
using CrisisManagement.Data.Models.Identity;
using CrisisManagement.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CrisisManagement.Data.Repositories;

public sealed class PasswordChangeLogRepository(AppDbContext context) : Repository<PasswordChangeLog>(context), IPasswordChangeLogRepository
{
    public Task<List<string>> GetRecentHashesAsync(int userId, int take) =>
        _entities.AsNoTracking()
            .Where(x => x.UserId == userId && x.PasswordHash != null)
            .OrderByDescending(x => x.CreatedOn).Take(take)
            .Select(x => x.PasswordHash!).ToListAsync();

    public Task<List<PasswordChangeLog>> GetForUserAsync(int userId) =>
        _entities.Where(x => x.UserId == userId).ToListAsync();
}
