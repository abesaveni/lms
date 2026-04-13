using LiveExpert.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LiveExpert.Infrastructure.Data.Configurations;

public class StudentProfileConfiguration : IEntityTypeConfiguration<StudentProfile>
{
    public void Configure(EntityTypeBuilder<StudentProfile> builder)
    {
        builder.ToTable("StudentProfiles");

        builder.HasKey(sp => sp.Id);

        builder.Property(sp => sp.LearningGoals)
            .HasMaxLength(1000);

        builder.Property(sp => sp.PreferredSubjects)
            .HasMaxLength(500);

        builder.Property(sp => sp.ReferralCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(sp => sp.CalendarProvider)
            .HasConversion<string>()
            .HasMaxLength(50);

        // Indexes
        builder.HasIndex(sp => sp.UserId).IsUnique();
        builder.HasIndex(sp => sp.ReferralCode).IsUnique();
    }
}

public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.ToTable("Subjects");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Description)
            .HasMaxLength(500);

        builder.Property(s => s.IconUrl)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(s => s.Name);
        builder.HasIndex(s => s.CategoryId);

        // Relationships
        builder.HasOne(s => s.Category)
            .WithMany(c => c.Subjects)
            .HasForeignKey(s => s.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.IconUrl)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(c => c.Name).IsUnique();
    }
}

public class SessionBookingConfiguration : IEntityTypeConfiguration<SessionBooking>
{
    public void Configure(EntityTypeBuilder<SessionBooking> builder)
    {
        builder.ToTable("SessionBookings");

        builder.HasKey(sb => sb.Id);

        builder.Property(sb => sb.BookingStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(sb => sb.BaseAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(sb => sb.PlatformFee)
            .HasColumnType("decimal(18,2)");

        builder.Property(sb => sb.TotalAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(sb => sb.SpecialInstructions)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(sb => sb.SessionId);
        builder.HasIndex(sb => sb.StudentId);
        builder.HasIndex(sb => sb.BookingStatus);
        builder.HasIndex(sb => new { sb.SessionId, sb.StudentId });
    }
}

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Content)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(m => m.MessageType)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Indexes
        builder.HasIndex(m => m.ConversationId);
        builder.HasIndex(m => m.SenderId);
        builder.HasIndex(m => m.CreatedAt);
    }
}

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("Conversations");

        builder.HasKey(c => c.Id);

        // Ignore navigation property that doesn't have a foreign key
        builder.Ignore(c => c.LastMessage);

        // Indexes
        builder.HasIndex(c => c.User1Id);
        builder.HasIndex(c => c.User2Id);
        builder.HasIndex(c => new { c.User1Id, c.User2Id }).IsUnique();
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(n => n.Message)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(n => n.NotificationType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(n => n.Priority)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(n => n.ActionUrl)
            .HasMaxLength(500);

        builder.Property(n => n.Metadata)
            .HasColumnType("TEXT");

        // Indexes
        builder.HasIndex(n => n.UserId);
        builder.HasIndex(n => n.IsRead);
        builder.HasIndex(n => n.CreatedAt);
    }
}

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.BaseAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.PlatformFee)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.TotalAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.PaymentMethod)
            .HasMaxLength(50);

        builder.Property(p => p.GatewayPaymentId)
            .HasMaxLength(255);

        builder.Property(p => p.GatewayOrderId)
            .HasMaxLength(255);

        builder.Property(p => p.Currency)
            .HasMaxLength(10);

        // Indexes
        builder.HasIndex(p => p.StudentId);
        builder.HasIndex(p => p.TutorId);
        builder.HasIndex(p => p.SessionId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.GatewayPaymentId);
    }
}

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Rating)
            .IsRequired();

        builder.Property(r => r.Comment)
            .HasMaxLength(1000);

        builder.Property(r => r.TutorResponse)
            .HasMaxLength(1000);

        // Fix relationships
        builder.HasOne(r => r.Student)
            .WithMany(u => u.GivenReviews)
            .HasForeignKey(r => r.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Tutor)
            .WithMany(u => u.ReceivedReviews)
            .HasForeignKey(r => r.TutorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(r => r.TutorId);
        builder.HasIndex(r => r.StudentId);
        builder.HasIndex(r => r.SessionId);
        builder.HasIndex(r => r.Rating);
    }
}
