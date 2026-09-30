using CMS.Data.Context;
using CMS.Features.PublicFiles.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CMS.Data;

// Ported from SafetyNet (Data/UnitOfWork/UnitOfWork.cs): repositories are created lazily, one per context.
public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    private readonly AppDbContext _context = context;

    private IPublicFilesRepository? _publicFiles;

    public IPublicFilesRepository PublicFiles => _publicFiles ??= new PublicFilesRepository(_context);

    public void SetCommandTimeout(int seconds) => _context.Database.SetCommandTimeout(seconds);

    public int SaveChanges() => _context.SaveChanges();

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
}
