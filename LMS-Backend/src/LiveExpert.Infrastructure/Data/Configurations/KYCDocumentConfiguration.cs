using LiveExpert.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LiveExpert.Infrastructure.Data.Configurations;

public class KYCDocumentConfiguration : IEntityTypeConfiguration<KYCDocument>
{
    public void Configure(EntityTypeBuilder<KYCDocument> builder)
    {
        builder.ToTable("KYCDocuments");

        builder.HasKey(k => k.Id);

        builder.Property(k => k.DocumentType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(k => k.DocumentNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(k => k.DocumentUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(k => k.VerificationStatus)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Fix relationship
        builder.HasOne(k => k.User)
            .WithMany()
            .HasForeignKey(k => k.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(k => k.UserId);
        builder.HasIndex(k => k.VerificationStatus);
    }
}
