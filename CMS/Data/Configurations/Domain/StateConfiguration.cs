using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class StateConfiguration : AuditableEntityConfiguration<State>
{
    public override void Configure(EntityTypeBuilder<State> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.StateId);

        entity.ToTable("CMS_State", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.StateDescription).HasMaxLength(50);
        entity.Property(e => e.StateCode)
            .HasMaxLength(2)
            .IsUnicode(false)
            .IsFixedLength();
        entity.Property(e => e.StateDescription)
            .HasColumnName("State");

    }
}

