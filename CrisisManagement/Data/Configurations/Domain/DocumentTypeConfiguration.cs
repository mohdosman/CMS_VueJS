using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Domain;

public class DocumentTypeConfiguration : AuditableEntityConfiguration<DocumentType>
{
    public override void Configure(EntityTypeBuilder<DocumentType> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);

        entity.ToTable("CMS_DocumentType", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.DocumentTypeName)
            .HasMaxLength(255)
            .IsUnicode(false);

    }
}
