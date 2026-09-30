using CrisisManagement.Data.Constants;
using CrisisManagement.Shared.Constants;
using CrisisManagement.Data.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Identity;

public sealed class MenuItemConfiguration : AuditableEntityConfiguration<MenuItem>
{
    public override void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        // Configure audit properties from base
        base.Configure(builder);

        // Table mapping
        builder.ToTable("RBS_MenuItem", DatabaseConstants.DefaultRoleBaseSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        // Entity-specific properties
        builder.Property(e => e.Icon)
            .IsRequired()
            .HasMaxLength(MenuFieldLimits.MaxIconLength)
            .IsUnicode(false);

        builder.Property(e => e.MenuItemName)
            .IsRequired()
            .HasMaxLength(MenuFieldLimits.MaxNameLength)
            .IsUnicode(false);

        builder.Property(e => e.Description)
            .HasMaxLength(MenuFieldLimits.MaxTextLength)
            .IsUnicode(false);

        builder.Property(e => e.Comment)
            .HasMaxLength(MenuFieldLimits.MaxTextLength)
            .IsUnicode(false);

        // URL properties
        builder.Property(e => e.Url)
            .HasMaxLength(MenuFieldLimits.MaxUrlLength)
            .IsUnicode(false);

        builder.Property(e => e.DetailUrl)
            .HasMaxLength(MenuFieldLimits.MaxUrlLength)
            .IsUnicode(false);

        builder.Property(e => e.TemplateUrl)
            .HasMaxLength(MenuFieldLimits.MaxUrlLength)
            .IsUnicode(false);

        builder.Property(e => e.DetailTemplateUrl)
            .HasMaxLength(MenuFieldLimits.MaxUrlLength)
            .IsUnicode(false);

        builder.Property(e => e.ApiUrl)
            .HasMaxLength(MenuFieldLimits.MaxUrlLength)
            .IsUnicode(false);

        builder.Property(e => e.IsEnabled)
            .HasDefaultValue(true);

        // Indexes
        builder.HasIndex(e => e.MenuItemName)
            .HasDatabaseName("IX_MenuItem")
            .IsUnique();
    }
}
