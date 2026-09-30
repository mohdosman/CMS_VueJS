using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Domain;

public class FacilityUserConfiguration : AuditableEntityConfiguration<FacilityUser>
{
    public override void Configure(EntityTypeBuilder<FacilityUser> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.FacilityUserId);

        entity.ToTable("CMS_FacilityUser", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.HasOne(d => d.Facility).WithMany(p => p.FacilityUsers)
            .HasForeignKey(d => d.FacilityId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_PQS1_FacilityUser_PQS1_Facility");

        entity.HasOne(d => d.User).WithMany(p => p.FacilityUsers)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_PQS1_FacilityUser_RBS_User");

    }
}

