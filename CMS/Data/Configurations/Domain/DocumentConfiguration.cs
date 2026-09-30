using CMS.Data.Constants;
using CMS.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Domain;

public class DocumentConfiguration : AuditableEntityConfiguration<Document>
{
    public override void Configure(EntityTypeBuilder<Document> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);
        entity.HasKey(e => e.DocumentId);

        entity.ToTable("CMS_Document", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.Property(e => e.FileName)
            .HasMaxLength(255)
            .IsUnicode(false);
        entity.Property(e => e.MIMEType)
            .HasMaxLength(50)
            .IsUnicode(false);

        entity.HasOne(d => d.DocumentType).WithMany(p => p.Documents)
            .HasForeignKey(d => d.DocumentTypeId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_CMS_Document_CMS_DocumentType");

    }
}

