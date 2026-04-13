using LiveExpert.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LiveExpert.Infrastructure.Data.Configurations;

public class TutorGoogleTokensConfiguration : IEntityTypeConfiguration<TutorGoogleTokens>
{
    public void Configure(EntityTypeBuilder<TutorGoogleTokens> builder)
    {
        builder.ToTable("TutorGoogleTokens");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.TutorId)
            .IsRequired();

        builder.Property(t => t.AccessToken)
            .IsRequired()
            .HasMaxLength(2000); // Encrypted tokens can be longer

        builder.Property(t => t.RefreshToken)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(t => t.TokenExpiry)
            .IsRequired();

        builder.Property(t => t.GoogleEmail)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(t => t.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Index for quick lookup
        builder.HasIndex(t => t.TutorId)
            .IsUnique()
            .HasFilter("[IsActive] = 1");

        // Foreign key relationship
        builder.HasOne(t => t.Tutor)
            .WithMany()
            .HasForeignKey(t => t.TutorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
