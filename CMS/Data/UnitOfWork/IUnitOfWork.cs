using CMS.Features.PublicFiles.Repositories;

namespace CMS.Data;

// Ported from SafetyNet (Data/UnitOfWork/IUnitOfWork.cs). Services reach data only through this:
// one property per repository. Add a repository here as each feature is moved onto the pattern.
public interface IUnitOfWork
{
    IPublicFilesRepository PublicFiles { get; }

    void SetCommandTimeout(int seconds);
    int SaveChanges();
    Task<int> SaveChangesAsync();
}
