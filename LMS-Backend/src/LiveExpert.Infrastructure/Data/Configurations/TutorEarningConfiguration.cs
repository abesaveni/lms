using LiveExpert.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LiveExpert.Infrastructure.Data.Configurations;

public class TutorEarningConfiguration : IEntityTypeConfiguration<TutorEarning>
{
    public void Configure(EntityTypeBuilder<TutorEarning> builder)
    {
        builder.ToTable("TutorEarnings");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TutorId)
            .IsRequired();

        builder.Property(e => e.SourceType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.CommissionPercentage)
            .HasPrecision(5, 2)
            .HasDefaultValue(0);

        builder.Property(e => e.CommissionAmount)
            .HasPrecision(18, 2)
            .HasDefaultValue(0);

        // Indexes
        builder.HasIndex(e => e.TutorId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => new { e.SourceType, e.SourceId });

        // Foreign keys
        builder.HasOne(e => e.Tutor)
            .WithMany()
            .HasForeignKey(e => e.TutorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.PayoutRequest)
            .WithMany(p => p.Earnings)
            .HasForeignKey(e => e.PayoutRequestId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
