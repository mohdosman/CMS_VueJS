using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Identity
{
    public class LogonConfiguration : IEntityTypeConfiguration<Logon>
    {
        public void Configure(EntityTypeBuilder<Logon> builder)
        {
            builder.ToTable("RBS_Logon", DatabaseConstants.DefaultRoleBaseSchema, t =>
            {
                t.UseSqlOutputClause(false);
            });

            builder.Property(e => e.LogOffDateTime).HasColumnType("datetime");
            builder.Property(e => e.LogOnDateTime).HasColumnType("datetime");
            builder.Property(e => e.SessionId).IsRequired().HasMaxLength(256);
            builder.Property(e => e.UserName).IsRequired().HasMaxLength(50);

            builder.Property(e => e.Version)
                    .HasColumnName("Version")
                    .IsRowVersion()
                    .IsConcurrencyToken();

            builder.HasOne(d => d.User)
                .WithMany(p => p.Logons)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Logon_User");
        }
    }
}
