using Elisal.WasteManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Elisal.WasteManagement.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transacao");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.Date).HasColumnName("data");
        builder.Property(e => e.AmountKg).HasColumnName("quantidade_kg");
        builder.Property(e => e.Value).HasColumnName("valor").HasColumnType("decimal(18,2)");
        builder.Property(e => e.Status)
            .HasColumnName("status")
            .HasMaxLength(50)
            .HasConversion<string>();

        builder.Property(e => e.WasteTypeId).HasColumnName("tipo_residuo_id");
        builder.Property(e => e.CooperativeId).HasColumnName("cooperativa_id");

        // Relationships
        builder.HasOne(e => e.WasteType)
            .WithMany()
            .HasForeignKey(e => e.WasteTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Cooperative)
            .WithMany()
            .HasForeignKey(e => e.CooperativeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
