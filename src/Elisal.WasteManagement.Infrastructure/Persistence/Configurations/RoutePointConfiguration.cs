using Elisal.WasteManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Elisal.WasteManagement.Infrastructure.Persistence.Configurations;

public class RoutePointConfiguration : IEntityTypeConfiguration<RoutePoint>
{
    public void Configure(EntityTypeBuilder<RoutePoint> builder)
    {
        builder.ToTable("RotaPonto");

        // Composite Key based on RotaPonto logic usually involves RouteId and CollectionPointId
        // The user request didn't explicitly specify an ID for RoutePoint table, just (rota_id, ponto_recolha_id, ordem_sequencia).
        // I'll assume composite key (RouteId, CollectionPointId) or just a keyless entity if intended, 
        // but typically it's a many-to-many join table with payload.
        
        builder.HasKey(e => new { e.RouteId, e.CollectionPointId });

        builder.Property(e => e.RouteId).HasColumnName("rota_id");
        builder.Property(e => e.CollectionPointId).HasColumnName("ponto_recolha_id");
        builder.Property(e => e.SequenceOrder).HasColumnName("ordem_sequencia");

        builder.HasOne(e => e.Route)
            .WithMany(r => r.RoutePoints)
            .HasForeignKey(e => e.RouteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.CollectionPoint)
            .WithMany()
            .HasForeignKey(e => e.CollectionPointId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
