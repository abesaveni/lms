using LiveExpert.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LiveExpert.Infrastructure.Data.Configurations;

public class ApiSettingConfiguration : IEntityTypeConfiguration<ApiSetting>
{
    public void Configure(EntityTypeBuilder<ApiSetting> builder)
    {
        builder.ToTable("ApiSettings");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Provider)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(a => a.KeyName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.KeyValue)
            .IsRequired()
            .HasMaxLength(2000); // Encrypted values can be longer

        builder.Property(a => a.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(a => a.Description)
            .HasMaxLength(500);

        // Index for quick lookup
        builder.HasIndex(a => new { a.Provider, a.KeyName })
            .IsUnique()
            .HasFilter("[IsActive] = 1");
    }
}
