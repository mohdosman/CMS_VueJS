using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class HospAltDispositionConfiguration : AuditableEntityConfiguration<HospAltDisposition>
{
    public override void Configure(EntityTypeBuilder<HospAltDisposition> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.HospAltDispositionId);

        entity.ToTable("CMS_HospAltDisposition", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.HasOne(d => d.HospAltDispositionList).WithMany(p => p.HospAltDispositions)
            .HasForeignKey(d => d.HospAltDispositionListId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_HospAltList_HospAltListId");

        entity.HasOne(d => d.HospitalizationAlternative).WithMany(p => p.HospAltDispositions)
            .HasForeignKey(d => d.HospitalizationAlternativeId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_HospAlt_HospAltId");

    }
}

