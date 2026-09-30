using CMS.Data.Constants;
using CMS.Data.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Data.Configurations.Identity
{
    public class TempTriggerConfiguration : IEntityTypeConfiguration<TempTrigger>
    {
        public void Configure(EntityTypeBuilder<TempTrigger> builder)
        {
            builder.ToTable("CMS_TempTrigger", DatabaseConstants.DefaultRoleBaseSchema, t =>
            {
                t.UseSqlOutputClause(false);
            });

            builder.HasKey(e => e.SPId); 
            builder.Property(e => e.SPId).HasColumnName("SPID").ValueGeneratedNever();
        }
    }
}
