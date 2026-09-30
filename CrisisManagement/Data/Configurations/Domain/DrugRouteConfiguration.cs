using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Domain;

public class DrugRouteConfiguration : AuditableEntityConfiguration<DrugRoute>
{
    public override void Configure(EntityTypeBuilder<DrugRoute> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.DrugRouteId);

        entity.ToTable("CMS_DrugRoute", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.DrugRouteDescription)
            .HasColumnName("DrugRoute")
            .HasMaxLength(150)
            .IsUnicode(false);
        entity.Property(e => e.NOMSId)
            .HasMaxLength(2)
            .IsUnicode(false)
            .IsFixedLength();

    }
}

