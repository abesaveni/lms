using LiveExpert.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LiveExpert.Infrastructure.Data.Configurations;

public class SessionMeetLinkConfiguration : IEntityTypeConfiguration<SessionMeetLink>
{
    public void Configure(EntityTypeBuilder<SessionMeetLink> builder)
    {
        builder.ToTable("SessionMeetLinks");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.SessionId)
            .IsRequired();

        builder.Property(s => s.MeetUrl)
            .IsRequired()
            .HasMaxLength(500); // Encrypted URL

        builder.Property(s => s.CalendarEventId)
            .HasMaxLength(255);

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Index for quick lookup
        builder.HasIndex(s => s.SessionId)
            .IsUnique()
            .HasFilter("[IsActive] = 1");

        // Foreign key relationship
        builder.HasOne(s => s.Session)
            .WithOne(s => s.MeetLink)
            .HasForeignKey<SessionMeetLink>(s => s.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
