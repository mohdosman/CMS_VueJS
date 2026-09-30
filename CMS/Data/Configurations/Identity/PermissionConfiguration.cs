using CMS.Data.Constants;
using CMS.Shared.Constants;
using CMS.Data.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Identity;

public sealed class PermissionConfiguration : AuditableEntityConfiguration<Permission>
{
    public override void Configure(EntityTypeBuilder<Permission> builder)
    {
        // Configure audit properties from base
        base.Configure(builder);

        // Table mapping
        builder.ToTable("RBS_Permission", DatabaseConstants.DefaultRoleBaseSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        // Entity-specific properties
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(PermissionFieldLimits.MaxPermissionTextLength)
            .IsUnicode(false);

        builder.Property(e => e.Value)
            .IsRequired()
            .HasMaxLength(PermissionFieldLimits.MaxPermissionTextLength)
            .IsUnicode(false);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(PermissionFieldLimits.MaxPermissionTextLength)
            .IsUnicode(false);

        // Relationships
        // Note: PermissionGroupName relationship is configured in PermissionGroupNameConfiguration (parent side)
        builder.HasOne(d => d.MenuItem)
            .WithMany(p => p.Permissions)
            .HasForeignKey(d => d.MenuItemId)
            .HasConstraintName("FK_Permission_MenuItem");
    }
}
