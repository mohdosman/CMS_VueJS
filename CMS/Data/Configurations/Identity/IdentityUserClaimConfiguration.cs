using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CMS.Data.Constants;

namespace CMS.Data.Configurations.Identity
{
    public sealed class IdentityUserClaimConfiguration : IEntityTypeConfiguration<IdentityUserClaim<int>>
    {
        public void Configure(EntityTypeBuilder<IdentityUserClaim<int>> entity)
        {
            entity.ToTable("RBS_UserClaim", DatabaseConstants.DefaultRoleBaseSchema, t =>
            {
                t.UseSqlOutputClause(false);
            });

            entity.Property(e => e.ClaimType).HasMaxLength(256);
            entity.Property(e => e.ClaimValue).HasMaxLength(1024);
            // The table column is the default "Id" (see migration.claims.sql.txt). The Blazor CMS mapped
            // it as UserClaimId, which does not exist, so any query on user claims failed there.
        }
    }
}
