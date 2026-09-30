using CMS.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations;

// Read-only here: menu/permission maintenance stays in the Blazor CMS until it is ported.
public sealed class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> b)
    {
        b.ToTable("RBS_MenuItem", "dbo");
        b.HasKey(e => e.MenuItemId);
        b.Property(e => e.Icon).HasMaxLength(50).IsUnicode(false);
        b.Property(e => e.MenuItemName).HasMaxLength(100).IsUnicode(false);
        b.Property(e => e.Url).IsUnicode(false);
    }
}

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> b)
    {
        b.ToTable("RBS_Permission", "dbo");
        b.HasKey(e => e.PermissionId);
        b.Property(e => e.Value).IsUnicode(false);
        b.HasOne<MenuItem>().WithMany(m => m.Permissions).HasForeignKey(p => p.MenuItemId);
    }
}
