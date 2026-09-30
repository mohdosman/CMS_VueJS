using CrisisManagement.Data.Constants;
using CrisisManagement.Shared.Constants;
using CrisisManagement.Data.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Identity;

public sealed class PermissionGroupConfiguration : AuditableEntityConfiguration<PermissionGroup>
{
    public override void Configure(EntityTypeBuilder<PermissionGroup> builder)
    {
        // Configure audit properties from base
        base.Configure(builder);

        // Table mapping
        builder.ToTable("RBS_PermissionGroup", DatabaseConstants.DefaultRoleBaseSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        // Primary key
        builder.HasKey(e => e.PermissionGroupId);

        // Entity-specific properties
        builder.Property(e => e.GroupName)
            .IsRequired()
            .HasMaxLength(PermissionFieldLimits.MaxGroupNameLength)
            .IsUnicode(false);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(PermissionFieldLimits.MaxGroupDescriptionLength)
            .IsUnicode(false);

        // Indexes
        builder.HasIndex(e => e.GroupName)
            .HasDatabaseName("IX_PermissionGroupName_GroupName")
            .IsUnique();

        // One-to-many relationship with Permission
        builder.HasMany(e => e.Permissions)
            .WithOne(p => p.PermissionGroupName)
            .HasForeignKey(p => p.PermissionGroupNameId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Permission_PermissionGroupName");
    }
}
