using CMS.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations;

// Table/column mapping copied from the Blazor CMS (CrisisMgmt.Server/Data/Configurations/Identity).
// UseSqlOutputClause(false): the tables have triggers.
public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> b)
    {
        b.ToTable("RBS_User", "dbo", t => t.UseSqlOutputClause(false));
        b.Property(e => e.Id).HasColumnName("UserId");
        b.Property(e => e.UserName).HasMaxLength(50).IsRequired();
        b.Property(e => e.NormalizedUserName).HasMaxLength(50);
        b.Property(e => e.Email).HasColumnType("varchar(100)");
        b.Property(e => e.NormalizedEmail).HasColumnType("varchar(100)");
        b.Property(e => e.PasswordHash).HasMaxLength(450);
        b.Property(e => e.SecurityStamp).HasMaxLength(64);
        b.Property(e => e.ConcurrencyStamp).HasMaxLength(64);
        b.Property(e => e.PhoneNumber).HasMaxLength(32);
        b.Property(e => e.LegacyPassword).HasColumnName("Password").HasMaxLength(128);
        b.Property(e => e.LegacyIsLockedOut).HasColumnName("IsLockedOut");
        b.Property(e => e.LastLoginDate).HasColumnType("datetime");
        b.Property(e => e.Comment).HasColumnType("varchar(max)");
        foreach (var dt in new[] { nameof(ApplicationUser.LastPasswordChangedDate), nameof(ApplicationUser.LastLockoutDate),
                                   nameof(ApplicationUser.CreatedOn), nameof(ApplicationUser.UpdatedOn) })
            b.Property(dt).HasColumnType("datetime");
        // Optimistic concurrency: the edit form sends the RowVersion it loaded.
        b.Property(e => e.Version).IsRowVersion();
        b.Ignore(e => e.FullName);
    }
}

public sealed class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> b)
    {
        b.ToTable("RBS_Role", "dbo", t => t.UseSqlOutputClause(false));
        b.Property(r => r.Id).HasColumnName("RoleId");
        b.Property(r => r.Name).HasColumnName("RoleName").HasMaxLength(256).IsUnicode(false);
    }
}

public sealed class UserRoleConfiguration : IEntityTypeConfiguration<ApplicationUserRole>
{
    public void Configure(EntityTypeBuilder<ApplicationUserRole> b)
    {
        b.ToTable("RBS_UserInRole", "dbo", t => t.UseSqlOutputClause(false));
        b.Property(e => e.UserInRoleId).ValueGeneratedOnAdd();
        b.Property(e => e.CreatedOn).HasColumnType("datetime");
        b.Property(e => e.UpdatedOn).HasColumnType("datetime");
    }
}

public sealed class UserClaimConfiguration : IEntityTypeConfiguration<IdentityUserClaim<int>>
{
    // Dev DB column is the default "Id" (the Blazor CMS mapping says UserClaimId, which is not in this table).
    public void Configure(EntityTypeBuilder<IdentityUserClaim<int>> b) =>
        b.ToTable("RBS_UserClaim", "dbo", t => t.UseSqlOutputClause(false));
}

public sealed class RoleClaimConfiguration : IEntityTypeConfiguration<IdentityRoleClaim<int>>
{
    public void Configure(EntityTypeBuilder<IdentityRoleClaim<int>> b)
    {
        b.ToTable("RBS_RoleClaim", "dbo", t => t.UseSqlOutputClause(false));
        b.Property(e => e.Id).HasColumnName("RoleClaimId");
    }
}

public sealed class UserLoginConfiguration : IEntityTypeConfiguration<IdentityUserLogin<int>>
{
    public void Configure(EntityTypeBuilder<IdentityUserLogin<int>> b) =>
        b.ToTable("RBS_UserLogin", "dbo", t => t.UseSqlOutputClause(false));
}

public sealed class UserTokenConfiguration : IEntityTypeConfiguration<IdentityUserToken<int>>
{
    public void Configure(EntityTypeBuilder<IdentityUserToken<int>> b) =>
        b.ToTable("RBS_UserToken", "dbo", t => t.UseSqlOutputClause(false));
}
