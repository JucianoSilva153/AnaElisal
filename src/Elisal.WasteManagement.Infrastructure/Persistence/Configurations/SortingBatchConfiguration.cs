using Elisal.WasteManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Elisal.WasteManagement.Infrastructure.Persistence.Configurations;

public class SortingBatchConfiguration : IEntityTypeConfiguration<SortingBatch>
{
    public void Configure(EntityTypeBuilder<SortingBatch> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Notes).HasMaxLength(500);

        builder.HasOne(e => e.WasteReception)
            .WithMany(r => r.SortingBatches)
            .HasForeignKey(e => e.WasteReceptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Operator)
            .WithMany()
            .HasForeignKey(e => e.OperatorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.StartDateTime);
        builder.HasIndex(e => e.Status);
    }
}

public class SortingBatchOutputConfiguration : IEntityTypeConfiguration<SortingBatchOutput>
{
    public void Configure(EntityTypeBuilder<SortingBatchOutput> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Notes).HasMaxLength(500);
        builder.Property(e => e.WeightKg).IsRequired();

        builder.HasOne(e => e.SortingBatch)
            .WithMany(b => b.Outputs)
            .HasForeignKey(e => e.SortingBatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.WasteType)
            .WithMany()
            .HasForeignKey(e => e.WasteTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
