using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Identity
{
    public class ReportLogConfiguration : IEntityTypeConfiguration<ReportLog>
    {
        public void Configure(EntityTypeBuilder<ReportLog> builder)
        {
            builder.ToTable("RBS_ReportLog", DatabaseConstants.DefaultRoleBaseSchema, t =>
            {
                t.UseSqlOutputClause(false);
            });

            builder.Property(e => e.ReportName).IsRequired().HasMaxLength(250).IsUnicode(false);
            builder.Property(e => e.SecurityToken).IsUnicode(false);
            builder.Property(e => e.CreatedOn).HasColumnType("datetime").HasDefaultValueSql("(getdate())");
            builder.Property(e => e.UpdatedOn).HasColumnType("datetime").HasDefaultValueSql("(getdate())");
            builder.Property(e => e.Version).IsRequired().IsRowVersion();
        }
    }
}
