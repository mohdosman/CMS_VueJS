using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Domain;

public class DSM4CodeConfiguration : IEntityTypeConfiguration<DSM4Code>
{
    public void Configure(EntityTypeBuilder<DSM4Code> entity)
    {

        entity.HasKey(e => e.DSM4CodeId);

        entity.ToTable("CMS_DSM4Code", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.DSM4_CODE)
            .HasMaxLength(10)
            .IsUnicode(false);
        entity.Property(e => e.ICD9_CODE)
            .HasMaxLength(40)
            .IsUnicode(false);
        entity.Property(e => e.axis_code)
            .HasMaxLength(5)
            .IsUnicode(false);
        entity.Property(e => e.axis_value)
            .HasMaxLength(40)
            .IsUnicode(false);
        entity.Property(e => e.dsm_description)
            .HasMaxLength(80)
            .IsUnicode(false);
        entity.Property(e => e.dsm_table)
            .HasMaxLength(10)
            .IsUnicode(false);
        entity.Property(e => e.dss_facility_id).HasColumnType("numeric(18, 0)");

        entity.HasOne(d => d.DSM4CodeType).WithMany(p => p.DSM4Codes)
            .HasForeignKey(d => d.DSM4CodeTypeId)
            .HasConstraintName("FK_CMS_DSM4Code_CMS_DSM4CodeType");

    }
}

