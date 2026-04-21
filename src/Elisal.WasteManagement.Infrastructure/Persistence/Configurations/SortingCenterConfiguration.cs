using Elisal.WasteManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Elisal.WasteManagement.Infrastructure.Persistence.Configurations;

public class SortingCenterConfiguration : IEntityTypeConfiguration<SortingCenter>
{
    public void Configure(EntityTypeBuilder<SortingCenter> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(150);
        builder.Property(e => e.Address).HasMaxLength(255);
        builder.Property(e => e.Municipality).HasMaxLength(100);
        builder.Property(e => e.Contact).HasMaxLength(50);
        builder.HasIndex(e => e.Name);
    }
}
