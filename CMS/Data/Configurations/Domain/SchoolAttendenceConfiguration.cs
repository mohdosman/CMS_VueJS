using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class SchoolAttendenceConfiguration : AuditableEntityConfiguration<SchoolAttendence>
{
    public override void Configure(EntityTypeBuilder<SchoolAttendence> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);

        entity.ToTable("CMS_SchoolAttendence", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.NOMSId)
            .HasMaxLength(2)
            .IsUnicode(false)
            .IsFixedLength();
        entity.Property(e => e.SchoolAttendenceDescription)
            .HasColumnName("SchoolAttendence")
            .HasMaxLength(150)
            .IsUnicode(false);
        entity.Property(e => e.SchoolAttendenceCode)
            .HasMaxLength(50)
            .IsUnicode(false);

    }
}

