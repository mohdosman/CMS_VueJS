using CrisisManagement.Data.Context;
using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Data.Repositories.Interfaces;
using CrisisManagement.Features.Providers.ViewModels;
using CrisisManagement.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace CrisisManagement.Data.Repositories;

public sealed class ProviderRepository(AppDbContext context) : Repository<Provider>(context), IProviderRepository
{
    private readonly AppDbContext _db = context;

    public Task<List<IdName>> GetIdNamesAsync(IReadOnlyCollection<int>? onlyIds = null)
    {
        var q = _entities.AsNoTracking();
        if (onlyIds is not null) q = q.Where(p => onlyIds.Contains(p.ProviderId));
        return q.OrderBy(p => p.Name).Select(p => new IdName(p.ProviderId, p.Name, p.Abbreviation)).ToListAsync();
    }

    public async Task<PagedResult<ProviderListItem>> SearchAsync(ProviderSearchRequest req, IReadOnlyCollection<int>? onlyIds)
    {
        var q = _entities.AsNoTracking();
        if (onlyIds is not null) q = q.Where(p => onlyIds.Contains(p.ProviderId));

        if (Has(req.Name)) { var v = LikePattern.Contains(req.Name!); q = q.Where(p => EF.Functions.Like(p.Name, v, LikePattern.Escape)); }
        if (Has(req.Abbreviation)) { var v = LikePattern.Contains(req.Abbreviation!); q = q.Where(p => EF.Functions.Like(p.Abbreviation, v, LikePattern.Escape)); }
        if (Has(req.EdisonNumber)) { var v = req.EdisonNumber!.Trim(); q = q.Where(p => p.EdisonNumber == v); }
        if (Has(req.Npi)) { var v = req.Npi!.Trim(); q = q.Where(p => p.Npi == v); }

        var desc = req.SortDesc;
        q = (req.SortBy ?? "").ToLowerInvariant() switch
        {
            "abbreviation" => desc ? q.OrderByDescending(p => p.Abbreviation).ThenBy(p => p.Name) : q.OrderBy(p => p.Abbreviation).ThenBy(p => p.Name),
            "edisonnumber" => desc ? q.OrderByDescending(p => p.EdisonNumber).ThenBy(p => p.Name) : q.OrderBy(p => p.EdisonNumber).ThenBy(p => p.Name),
            "npi" => desc ? q.OrderByDescending(p => p.Npi).ThenBy(p => p.Name) : q.OrderBy(p => p.Npi).ThenBy(p => p.Name),
            _ => desc ? q.OrderByDescending(p => p.Name) : q.OrderBy(p => p.Name)
        };

        var size = Math.Clamp(req.PageSize, 1, 200);
        var page = Math.Max(1, req.PageIndex);
        var total = await q.CountAsync();
        var items = await q.Skip((page - 1) * size).Take(size)
            .Select(p => new ProviderListItem(p.ProviderId, p.Name, p.Abbreviation, p.EdisonNumber, p.Npi, p.UpdatedOn))
            .ToListAsync();
        return new PagedResult<ProviderListItem> { Items = items, TotalCount = total };
    }

    public Task<Provider?> GetAggregateAsync(int id) =>
        _entities.Include(p => p.ProviderAddresses).ThenInclude(pa => pa.Address).ThenInclude(a => a.Contact)
            .FirstOrDefaultAsync(p => p.ProviderId == id);

    public Task<bool> NpiExistsAsync(string npi, int exceptId) => _entities.AnyAsync(p => p.Npi == npi && p.ProviderId != exceptId);

    public Task<bool> EdisonExistsAsync(string edisonNumber, int exceptId) =>
        _entities.AnyAsync(p => p.EdisonNumber == edisonNumber && p.ProviderId != exceptId);

    public Task<bool> HasReferencesAsync(int id) =>
        _entities.AnyAsync(p => p.ProviderId == id &&
            (p.ProviderUsers.Any() || p.Contracts.Any() || p.F2FAssessments.Any() || p.PhoneAssessments.Any() || p.ServiceFiles.Any() || p.Services.Any()));

    public Task<IdName?> GetIdNameByNpiAsync(string npi) =>
        _entities.AsNoTracking().Where(p => p.Npi == npi).Select(p => new IdName(p.ProviderId, p.Name, p.Abbreviation)).FirstOrDefaultAsync();

    public Task<List<LookupItem>> GetStatesAsync() =>
        _db.Set<State>().AsNoTracking().OrderBy(s => s.StateDescription)
            .Select(s => new LookupItem(s.StateId, s.StateDescription, s.StateCode)).ToListAsync();

    public Task<List<LookupItem>> GetCountiesAsync() =>
        _db.Set<County>().AsNoTracking().OrderBy(c => c.CountyDescription)
            .Select(c => new LookupItem(c.CountyId, c.CountyDescription, c.CountyCode)).ToListAsync();

    public void RemoveAddressGraph(ProviderAddress providerAddress)
    {
        if (providerAddress.Address?.Contact is { } contact) _db.Remove(contact);
        if (providerAddress.Address is { } address) _db.Remove(address);
        _db.Remove(providerAddress);
    }

    public void RemoveContact(Contact contact) => _db.Remove(contact);

    private static bool Has(string? s) => !string.IsNullOrWhiteSpace(s);
}
