using CMS.Data.Constants;
using CMS.Data.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Identity;

public sealed class ApplicationUserRoleConfiguration : AuditableEntityConfiguration<ApplicationUserRole>
{
    public override void Configure(EntityTypeBuilder<ApplicationUserRole> builder)
    {
        // Configure audit properties from base
        base.Configure(builder);

        // Table mapping
        builder.ToTable("RBS_UserInRole", DatabaseConstants.DefaultRoleBaseSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        // Composite primary key
        builder.HasKey(ur => new { ur.UserId, ur.RoleId });

        // Properties
        builder.Property(x => x.UserInRoleId)
            .HasColumnName("UserInRoleId")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UserId)
            .HasColumnName("UserId")
            .IsRequired();

        builder.Property(x => x.RoleId)
            .HasColumnName("RoleId")
            .IsRequired();

        // Helpful indexes
        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_RBS_UserInRole_UserId");

        builder.HasIndex(x => x.RoleId)
            .HasDatabaseName("IX_RBS_UserInRole_RoleId");

        builder.HasIndex(x => new { x.UserId, x.RoleId })
           .IsUnique()
           .HasDatabaseName("UX_RBS_UserInRole_User_Role");
    }
}
