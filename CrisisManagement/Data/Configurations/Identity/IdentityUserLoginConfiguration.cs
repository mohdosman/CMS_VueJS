using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CrisisManagement.Data.Constants;

namespace CrisisManagement.Data.Configurations.Identity
{
    public sealed class IdentityUserLoginConfiguration : IEntityTypeConfiguration<IdentityUserLogin<int>>
    {
        public void Configure(EntityTypeBuilder<IdentityUserLogin<int>> entity)
        {
            entity.ToTable("RBS_UserLogin", DatabaseConstants.DefaultRoleBaseSchema, t =>
            {
                t.UseSqlOutputClause(false);
            });

            entity.Property(e => e.ProviderDisplayName).HasMaxLength(256);
        }
    }
}
