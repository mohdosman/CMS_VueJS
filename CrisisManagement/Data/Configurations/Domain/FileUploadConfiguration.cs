using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Domain;

public sealed class FileUploadConfiguration : AuditableEntityConfiguration<FileUpload>
{
    public override void Configure(EntityTypeBuilder<FileUpload> entity)
    {
        // Configure audit properties from base
        base.Configure(entity);

        // Table configuration
        entity.ToTable("CMS_FileUpload", DatabaseConstants.DefaultSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        entity.HasKey(x => x.FileUploadId);

        // File properties
        entity.Property(x => x.FileName)
            .IsRequired()
            .HasMaxLength(255);

        entity.Property(x => x.FileTextXML)
            .IsRequired()
            .HasColumnType("xml");

        entity.Property(x => x.ProviderNPI)
            .HasMaxLength(50);

        // Processing flags
        entity.Property(x => x.IsInProcess)
            .IsRequired()
            .HasDefaultValue(false);

        entity.Property(x => x.IsProcessed)
            .IsRequired()
            .HasDefaultValue(false);

        // Indexes for performance
        entity.HasIndex(x => x.ProviderNPI)
            .HasDatabaseName("IX_FileUpload_ProviderNPI");

        entity.HasIndex(x => x.IsProcessed)
            .HasDatabaseName("IX_FileUpload_IsProcessed");

        entity.HasIndex(x => new { x.IsInProcess, x.IsProcessed })
            .HasDatabaseName("IX_FileUpload_ProcessingStatus");
    }
}
