using LiveExpert.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LiveExpert.Infrastructure.Data.Configurations;

public class WhatsAppCampaignConfiguration : IEntityTypeConfiguration<WhatsAppCampaign>
{
    public void Configure(EntityTypeBuilder<WhatsAppCampaign> builder)
    {
        builder.ToTable("WhatsAppCampaigns");

        builder.HasKey(c => c.Id);

        // Configure Creator relationship
        // Database has CreatorId as the foreign key column
        // Map Creator navigation to use CreatorId (shadow property)
        builder.HasOne(c => c.Creator)
            .WithMany()
            .HasForeignKey("CreatorId")  // Use CreatorId as the foreign key
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        
        // Map CreatedBy property to also write to CreatorId
        // This ensures both CreatedBy and CreatorId are set to the same value
        builder.Property(c => c.CreatedBy)
            .IsRequired();

        // Indexes
        builder.HasIndex(c => c.CreatedBy);
        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.CreatedAt);
    }
}
