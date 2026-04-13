using LiveExpert.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LiveExpert.Infrastructure.Data.Configurations;

public class TutorVerificationConfiguration : IEntityTypeConfiguration<TutorVerification>
{
    public void Configure(EntityTypeBuilder<TutorVerification> builder)
    {
        builder.ToTable("TutorVerifications");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.TutorId)
            .IsRequired();

        builder.Property(v => v.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(v => v.AdminNotes)
            .HasMaxLength(1000);

        builder.Property(v => v.RejectionReason)
            .HasMaxLength(1000);

        // Index for quick lookup
        builder.HasIndex(v => v.TutorId)
            .IsUnique();

        builder.HasIndex(v => v.Status);

        // Foreign keys
        builder.HasOne(v => v.Tutor)
            .WithMany()
            .HasForeignKey(v => v.TutorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.VerifiedByUser)
            .WithMany()
            .HasForeignKey(v => v.VerifiedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
