using LiveExpert.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LiveExpert.Infrastructure.Data.Configurations;

public class UserCalendarConnectionConfiguration : IEntityTypeConfiguration<UserCalendarConnection>
{
    public void Configure(EntityTypeBuilder<UserCalendarConnection> builder)
    {
        builder.ToTable("UserCalendarConnections");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.UserId)
            .IsRequired();

        builder.Property(c => c.Provider)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(c => c.AccessToken)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(c => c.RefreshToken)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(c => c.TokenExpiry)
            .IsRequired();

        builder.Property(c => c.GoogleEmail)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Index for quick lookup - one active connection per user
        builder.HasIndex(c => c.UserId)
            .IsUnique()
            .HasFilter("[IsActive] = 1");

        // Foreign key relationship
        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
