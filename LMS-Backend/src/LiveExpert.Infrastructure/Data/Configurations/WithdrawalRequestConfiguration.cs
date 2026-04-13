using LiveExpert.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LiveExpert.Infrastructure.Data.Configurations;

public class WithdrawalRequestConfiguration : IEntityTypeConfiguration<WithdrawalRequest>
{
    public void Configure(EntityTypeBuilder<WithdrawalRequest> builder)
    {
        builder.ToTable("WithdrawalRequests");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Amount)
            .HasColumnType("decimal(18,2)");

        builder.Property(w => w.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Fix the ambiguous relationship
        builder.HasOne(w => w.User)
            .WithMany()
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.ProcessedByUser)
            .WithMany()
            .HasForeignKey(w => w.ProcessedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(w => w.UserId);
        builder.HasIndex(w => w.Status);
    }
}
