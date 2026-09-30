using CMS.Data.Context;
using CMS.Data.Models.Identity;
using CMS.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CMS.Data.Repositories;

public sealed class RoleRepository(AppDbContext context) : Repository<ApplicationRole>(context), IRoleRepository
{
    public Task<List<IdName>> GetIdNamesAsync(IReadOnlyCollection<int>? onlyIds = null)
    {
        var q = _entities.AsNoTracking();
        if (onlyIds is not null) q = q.Where(r => onlyIds.Contains(r.Id));
        return q.OrderBy(r => r.Name).Select(r => new IdName(r.Id, r.Name!)).ToListAsync();
    }

    public async Task<(List<IdName> Items, int Total)> SearchAsync(string? nameLike, string? sortBy, bool desc, int page, int size)
    {
        var q = _entities.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(nameLike)) { var v = $"%{nameLike.Trim()}%"; q = q.Where(r => EF.Functions.Like(r.Name!, v)); }

        var total = await q.CountAsync();
        q = (sortBy ?? "").ToLowerInvariant() switch
        {
            "id" => desc ? q.OrderByDescending(r => r.Id) : q.OrderBy(r => r.Id),
            _ => desc ? q.OrderByDescending(r => r.Name) : q.OrderBy(r => r.Name)
        };
        var items = await q.Skip((page - 1) * size).Take(size).Select(r => new IdName(r.Id, r.Name!)).ToListAsync();
        return (items, total);
    }

    public Task<ApplicationRole?> GetByIdAsync(int id) => _entities.FirstOrDefaultAsync(r => r.Id == id);

    public Task<bool> NameExistsAsync(string normalizedName, int exceptId) =>
        _entities.AnyAsync(r => r.NormalizedName == normalizedName && r.Id != exceptId);
}
