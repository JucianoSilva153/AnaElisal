using Elisal.WasteManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Elisal.WasteManagement.Infrastructure.Persistence.Configurations;

public class RouteExecutionConfiguration : IEntityTypeConfiguration<RouteExecution>
{
    public void Configure(EntityTypeBuilder<RouteExecution> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.HasOne(e => e.Route)
            .WithMany()
            .HasForeignKey(e => e.RouteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Driver)
            .WithMany()
            .HasForeignKey(e => e.DriverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.Status)
            .HasConversion<string>();
    }
}

public class RoutePointExecutionStatusConfiguration : IEntityTypeConfiguration<RoutePointExecutionStatus>
{
    public void Configure(EntityTypeBuilder<RoutePointExecutionStatus> builder)
    {
        builder.HasKey(s => s.Id);

        builder.HasOne(s => s.RouteExecution)
            .WithMany(e => e.PointStatuses)
            .HasForeignKey(s => s.RouteExecutionId);

        builder.HasOne(s => s.CollectionPoint)
            .WithMany()
            .HasForeignKey(s => s.CollectionPointId);
    }
}
