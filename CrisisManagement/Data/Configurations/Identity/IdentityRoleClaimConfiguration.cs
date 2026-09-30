using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CrisisManagement.Data.Constants;

namespace CrisisManagement.Data.Configurations.Identity
{
    public sealed class IdentityRoleClaimConfiguration : IEntityTypeConfiguration<IdentityRoleClaim<int>>
    {
        public void Configure(EntityTypeBuilder<IdentityRoleClaim<int>> entity)
        {
            entity.ToTable("RBS_RoleClaim", DatabaseConstants.DefaultRoleBaseSchema, t =>
            {
                t.UseSqlOutputClause(false);
            });

            entity.Property(e => e.ClaimType).HasMaxLength(256);
            entity.Property(e => e.ClaimValue).HasMaxLength(1024);
            entity.Property(e => e.Id).HasColumnName("RoleClaimId");
            entity.Property(e => e.RoleId).HasColumnName("RoleId");
        }
    }
}
