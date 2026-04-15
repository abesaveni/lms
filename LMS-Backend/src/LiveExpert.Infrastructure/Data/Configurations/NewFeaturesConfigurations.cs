using LiveExpert.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LiveExpert.Infrastructure.Data.Configurations;

// Feature 6: TimeSpan stored as TEXT in SQLite
public class TutorAvailabilityConfiguration : IEntityTypeConfiguration<TutorAvailability>
{
    public void Configure(EntityTypeBuilder<TutorAvailability> builder)
    {
        builder.ToTable("TutorAvailabilities");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.StartTime)
            .HasConversion<string>()
            .HasColumnType("TEXT");

        builder.Property(a => a.EndTime)
            .HasConversion<string>()
            .HasColumnType("TEXT");

        builder.HasOne(a => a.Tutor)
            .WithMany()
            .HasForeignKey(a => a.TutorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

// Feature 4: Explicit FK config to avoid ambiguous User references
public class StudentRatingConfiguration : IEntityTypeConfiguration<StudentRating>
{
    public void Configure(EntityTypeBuilder<StudentRating> builder)
    {
        builder.ToTable("StudentRatings");
        builder.HasKey(r => r.Id);

        builder.HasOne(r => r.Session)
            .WithMany()
            .HasForeignKey(r => r.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Tutor)
            .WithMany()
            .HasForeignKey(r => r.TutorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Student)
            .WithMany()
            .HasForeignKey(r => r.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

// Feature 12: TutorInquiry FK config for two User FKs
public class TutorInquiryConfiguration : IEntityTypeConfiguration<TutorInquiry>
{
    public void Configure(EntityTypeBuilder<TutorInquiry> builder)
    {
        builder.ToTable("TutorInquiries");
        builder.HasKey(i => i.Id);

        builder.HasOne(i => i.Student)
            .WithMany()
            .HasForeignKey(i => i.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Tutor)
            .WithMany()
            .HasForeignKey(i => i.TutorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
