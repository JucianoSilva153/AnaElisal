using Elisal.WasteManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Elisal.WasteManagement.Infrastructure.Persistence.Configurations;

public class CollectionRecordConfiguration : IEntityTypeConfiguration<CollectionRecord>
{
    public void Configure(EntityTypeBuilder<CollectionRecord> builder)
    {
        builder.ToTable("RegistoRecolha");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.DateTime).HasColumnName("data_hora");
        builder.Property(e => e.AmountKg).HasColumnName("quantidade_kg");
        builder.Property(e => e.Notes).HasColumnName("observacoes").HasMaxLength(500);

        builder.Property(e => e.WasteTypeId).HasColumnName("tipo_residuo_id");
        builder.Property(e => e.CollectionPointId).HasColumnName("ponto_recolha_id");
        builder.Property(e => e.UserId).HasColumnName("usuario_id");

        // Relationships
        builder.HasOne(e => e.WasteType)
            .WithMany()
            .HasForeignKey(e => e.WasteTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.CollectionPoint)
            .WithMany()
            .HasForeignKey(e => e.CollectionPointId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
