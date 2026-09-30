using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class ProviderUserConfiguration : AuditableEntityConfiguration<ProviderUser>
{
    public override void Configure(EntityTypeBuilder<ProviderUser> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.ProviderUserId);

        entity.ToTable("CMS_ProviderUser", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.HasOne(d => d.Provider).WithMany(p => p.ProviderUsers)
            .HasForeignKey(d => d.ProviderId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_CMS_ProviderUser_CMS_Provider");

        entity.HasOne(d => d.User).WithMany(p => p.ProviderUsers)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_CMS_ProviderUser_RBS_User");

    }
}

