using Elisal.WasteManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Elisal.WasteManagement.Infrastructure.Persistence.Configurations;

public class RouteConfiguration : IEntityTypeConfiguration<Route>
{
    public void Configure(EntityTypeBuilder<Route> builder)
    {
        builder.ToTable("Rota");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.Name).HasColumnName("nome").IsRequired().HasMaxLength(150);
        builder.Property(e => e.Description).HasColumnName("descricao").HasMaxLength(255);
        builder.Property(e => e.WeekDay).HasColumnName("dia_semana").HasMaxLength(20);
        builder.Property(e => e.StartTime).HasColumnName("hora_inicio");
    }
}
