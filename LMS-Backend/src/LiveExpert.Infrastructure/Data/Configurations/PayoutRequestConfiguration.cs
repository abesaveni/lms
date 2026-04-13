using LiveExpert.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LiveExpert.Infrastructure.Data.Configurations;

public class PayoutRequestConfiguration : IEntityTypeConfiguration<PayoutRequest>
{
    public void Configure(EntityTypeBuilder<PayoutRequest> builder)
    {
        builder.ToTable("PayoutRequests");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.TutorId)
            .IsRequired();

        builder.Property(p => p.BankAccountId)
            .IsRequired();

        builder.Property(p => p.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(p => p.RequestedAt)
            .IsRequired();

        builder.Property(p => p.PaymentMethod)
            .HasMaxLength(50)
            .HasDefaultValue("Bank Transfer");

        // Indexes
        builder.HasIndex(p => p.TutorId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.RequestedAt);

        // Foreign keys
        builder.HasOne(p => p.Tutor)
            .WithMany()
            .HasForeignKey(p => p.TutorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.BankAccount)
            .WithMany()
            .HasForeignKey(p => p.BankAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.ProcessedByUser)
            .WithMany()
            .HasForeignKey(p => p.ProcessedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
