using CMS.Features.PublicFiles.Repositories;

namespace CMS.Data.UnitOfWork;

// Services reach data only through this (SafetyNet pattern): one property per feature repository.
// Add a repository here as each feature is moved onto the pattern.
public interface IUnitOfWork
{
    IPublicFilesRepository PublicFiles { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
