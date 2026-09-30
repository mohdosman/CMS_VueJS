using CMS.Data.Constants;
using CMS.Data.Models.Identity;
using CMS.Shared.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Identity;

public sealed class ApplicationRoleConfiguration : AuditableEntityConfiguration<ApplicationRole>
{
    public override void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        // Configure audit properties from base
        base.Configure(builder);

        // Table mapping
        builder.ToTable("RBS_Role", DatabaseConstants.DefaultRoleBaseSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        // Primary key
        builder.HasKey(r => r.Id);

        // Properties
        builder.Property(r => r.Id)
            .HasColumnName("RoleId");

        builder.Property(r => r.RoleKey)
            .HasDefaultValueSql("(newid())");

        builder.Property(r => r.Name)
            .HasColumnName("RoleName")
            .HasMaxLength(RoleFieldLimits.MaxNameLength)
            .IsUnicode(false);

        // Relationships
        builder.HasMany(r => r.Users)
            .WithOne()
            .HasForeignKey(ur => ur.RoleId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Claims)
            .WithOne()
            .HasForeignKey(rc => rc.RoleId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
