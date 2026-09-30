using System.Security.Claims;
using CMS.Data.Models;
using CMS.Shared.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CMS.Data.Context;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor http)
    : IdentityDbContext<ApplicationUser, ApplicationRole, int,
        IdentityUserClaim<int>, ApplicationUserRole, IdentityUserLogin<int>,
        IdentityRoleClaim<int>, IdentityUserToken<int>>(options)
{
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<Provider> Providers => Set<Provider>();
    public DbSet<ProviderUser> ProviderUsers => Set<ProviderUser>();
    public DbSet<Logon> Logons => Set<Logon>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<PasswordChangeLog> PasswordChangeLogs => Set<PasswordChangeLog>();

    // Existing legacy tables; no EF migrations here - the CMS database is pre-existing.
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        Audit();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken ct = default)
    {
        Audit();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, ct);
    }

    // The CreatedBy/UpdatedBy columns are NOT NULL, so unauthenticated writes stamp 0.
    private void Audit()
    {
        var now = DateTime.Now;
        var uid = int.TryParse(http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

        foreach (var e in ChangeTracker.Entries<IAudited>())
        {
            if (e.State == EntityState.Added)
            {
                e.Entity.CreatedOn = e.Entity.UpdatedOn = now;
                e.Entity.CreatedBy = e.Entity.UpdatedBy = uid;
            }
            else if (e.State == EntityState.Modified)
            {
                e.Property(x => x.CreatedOn).IsModified = false;
                e.Property(x => x.CreatedBy).IsModified = false;
                e.Entity.UpdatedOn = now;
                e.Entity.UpdatedBy = uid;
            }
        }
    }
}
