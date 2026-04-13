using LiveExpert.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LiveExpert.Infrastructure.Data.Configurations;

public class TutorProfileConfiguration : IEntityTypeConfiguration<TutorProfile>
{
    public void Configure(EntityTypeBuilder<TutorProfile> builder)
    {
        builder.ToTable("TutorProfiles");

        builder.HasKey(tp => tp.Id);

        builder.Property(tp => tp.Bio)
            .HasMaxLength(2000);

        builder.Property(tp => tp.Headline)
            .HasMaxLength(200);

        builder.Property(tp => tp.HourlyRate)
            .HasColumnType("decimal(18,2)");

        builder.Property(tp => tp.VerificationStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(tp => tp.RejectionReason)
            .HasMaxLength(1000);

        builder.Property(tp => tp.ResumeUrl)
            .HasMaxLength(500);

        builder.Property(tp => tp.VideoIntroUrl)
            .HasMaxLength(500);

        builder.Property(tp => tp.AverageRating)
            .HasColumnType("decimal(3,2)");

        builder.Property(tp => tp.CompletionRate)
            .HasColumnType("decimal(5,2)");

        builder.Property(tp => tp.CalendarProvider)
            .HasConversion<string>()
            .HasMaxLength(50);

        // Indexes
        builder.HasIndex(tp => tp.UserId).IsUnique();
        builder.HasIndex(tp => tp.VerificationStatus);
        builder.HasIndex(tp => tp.AverageRating);
    }
}
