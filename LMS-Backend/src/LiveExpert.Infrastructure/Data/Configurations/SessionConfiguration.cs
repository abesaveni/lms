using LiveExpert.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LiveExpert.Infrastructure.Data.Configurations;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("Sessions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Description)
            .HasMaxLength(2000);

        builder.Property(s => s.SessionType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(s => s.MeetingLink)
            .HasMaxLength(500);

        builder.Property(s => s.RecordingUrl)
            .HasMaxLength(500);

        builder.Property(s => s.CalendarEventId)
            .HasMaxLength(255);

        // Indexes
        builder.HasIndex(s => s.TutorId);
        builder.HasIndex(s => s.SubjectId);
        builder.HasIndex(s => s.ScheduledAt);
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => new { s.TutorId, s.ScheduledAt });

        // Relationships
        builder.HasOne(s => s.Subject)
            .WithMany(sub => sub.Sessions)
            .HasForeignKey(s => s.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Bookings)
            .WithOne(sb => sb.Session)
            .HasForeignKey(sb => sb.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Reviews)
            .WithOne(r => r.Session)
            .HasForeignKey(r => r.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
