using CrisisManagement.Data.Constants;
using CrisisManagement.Data.Models.Auth;
using CrisisManagement.Data.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrisisManagement.Data.Configurations.Identity;

public sealed class AppSessionConfiguration : IEntityTypeConfiguration<AppSessionEntity>
{
    public void Configure(EntityTypeBuilder<AppSessionEntity> builder)
    {
        builder.ToTable("RBS_AppSessions", DatabaseConstants.DefaultRoleBaseSchema, t =>
        {
            t.UseSqlOutputClause(false);
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired()
            .HasMaxLength(32)
            .IsFixedLength(true); // NCHAR(32) for GUID strings

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.CreatedUtc)
            .IsRequired();

        builder.Property(x => x.AbsoluteExpiresUtc)
            .IsRequired();

        builder.Property(x => x.LastActivityUtc)
            .IsRequired();

        // Index for cleanup queries
        builder.HasIndex(x => x.AbsoluteExpiresUtc)
            .HasDatabaseName("IX_AppSessions_AbsoluteExpiresUtc");

        // Index for user lookup
        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_AppSessions_UserId");

        // Foreign key to Users table
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
