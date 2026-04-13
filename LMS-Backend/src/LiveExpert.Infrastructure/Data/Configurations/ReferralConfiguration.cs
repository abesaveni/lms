using LiveExpert.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LiveExpert.Infrastructure.Data.Configurations;

public class ReferralConfiguration : IEntityTypeConfiguration<Referral>
{
    public void Configure(EntityTypeBuilder<Referral> builder)
    {
        builder.ToTable("Referrals");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.ReferrerUserId)
            .IsRequired();

        builder.Property(r => r.ReferredUserId)
            .IsRequired();

        builder.Property(r => r.ReferralCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.BonusCredits)
            .IsRequired()
            .HasPrecision(18, 2);

        // Indexes
        builder.HasIndex(r => r.ReferralCode);
        builder.HasIndex(r => r.ReferrerUserId);
        builder.HasIndex(r => r.ReferredUserId);

        // Unique constraint: one referral per referred user
        builder.HasIndex(r => r.ReferredUserId)
            .IsUnique();

        // Foreign keys
        builder.HasOne(r => r.Referrer)
            .WithMany()
            .HasForeignKey(r => r.ReferrerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.ReferredUser)
            .WithMany()
            .HasForeignKey(r => r.ReferredUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
