using Elisal.WasteManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Elisal.WasteManagement.Infrastructure.Persistence.Configurations;

public class WasteReceptionConfiguration : IEntityTypeConfiguration<WasteReception>
{
    public void Configure(EntityTypeBuilder<WasteReception> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Notes).HasMaxLength(500);
        builder.Property(e => e.GrossWeightKg).IsRequired();
        builder.Property(e => e.NetWeightKgStored).IsRequired();

        builder.HasOne(e => e.SortingCenter)
            .WithMany()
            .HasForeignKey(e => e.SortingCenterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ReceivedByUser)
            .WithMany()
            .HasForeignKey(e => e.ReceivedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.RouteExecution)
            .WithMany()
            .HasForeignKey(e => e.RouteExecutionId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.HasIndex(e => e.DateTime);
        builder.HasIndex(e => e.IsSorted);
    }
}
