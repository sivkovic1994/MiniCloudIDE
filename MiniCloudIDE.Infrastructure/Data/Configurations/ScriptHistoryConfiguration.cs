using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniCloudIDE.Domain.Entities;

namespace MiniCloudIDE.Infrastructure.Data.Configurations
{
    public class ScriptHistoryConfiguration : IEntityTypeConfiguration<ScriptHistory>
    {
        public void Configure(EntityTypeBuilder<ScriptHistory> builder)
        {
            builder.ToTable("script_history");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Language)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(e => e.Code);

            builder.Property(e => e.CreatedAt)
                .HasDefaultValueSql("NOW()");

            builder.Property(e => e.UserId)
                .IsRequired();

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
