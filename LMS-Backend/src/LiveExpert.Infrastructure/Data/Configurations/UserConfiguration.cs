using LiveExpert.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LiveExpert.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(u => u.WhatsAppNumber)
            .HasMaxLength(20);

        builder.Property(u => u.PasswordHash)
            .IsRequired();

        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(u => u.GoogleId)
            .HasMaxLength(255);

        builder.Property(u => u.ProfileImageUrl)
            .HasMaxLength(500);

        // Additional user fields (added via schema repair in DbInitializer for existing databases)
        builder.Property(u => u.Bio);
        builder.Property(u => u.DateOfBirth);
        builder.Property(u => u.Location).HasMaxLength(255);
        builder.Property(u => u.Language).HasMaxLength(50);
        builder.Property(u => u.Timezone).HasMaxLength(100);

        // Indexes
        builder.HasIndex(u => u.Username).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.PhoneNumber);
        builder.HasIndex(u => u.GoogleId);

        // Relationships
        builder.HasOne(u => u.TutorProfile)
            .WithOne(tp => tp.User)
            .HasForeignKey<TutorProfile>(tp => tp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.StudentProfile)
            .WithOne(sp => sp.User)
            .HasForeignKey<StudentProfile>(sp => sp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.TutorSessions)
            .WithOne(s => s.Tutor)
            .HasForeignKey(s => s.TutorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.StudentBookings)
            .WithOne(sb => sb.Student)
            .HasForeignKey(sb => sb.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.SentMessages)
            .WithOne(m => m.Sender)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.ReceivedMessages)
            .WithOne(m => m.Receiver)
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
