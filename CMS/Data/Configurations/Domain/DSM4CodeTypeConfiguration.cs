using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class DSM4CodeTypeConfiguration : IEntityTypeConfiguration<DSM4CodeType>
{
    public void Configure(EntityTypeBuilder<DSM4CodeType> entity)
    {

        entity.ToTable("CMS_DSM4CodeType", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.DSM4CodeTypeDescription).HasMaxLength(50);
        entity.Property(e => e.DSM4CodeTypeDescription)
            .HasColumnName("DSM4CodeType");

    }
}

