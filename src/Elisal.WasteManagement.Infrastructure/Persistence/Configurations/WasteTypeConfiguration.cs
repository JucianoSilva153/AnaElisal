using Elisal.WasteManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Elisal.WasteManagement.Infrastructure.Persistence.Configurations;

public class WasteTypeConfiguration : IEntityTypeConfiguration<WasteType>
{
    public void Configure(EntityTypeBuilder<WasteType> builder)
    {
        builder.ToTable("TipoResiduo");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.Name).HasColumnName("nome").IsRequired().HasMaxLength(100);
        builder.Property(e => e.Description).HasColumnName("descricao").HasMaxLength(255);
        builder.Property(e => e.ColorCode).HasColumnName("cor_identificacao").HasMaxLength(20);
        builder.Property(e => e.IsRecyclable).HasColumnName("reciclavel");
    }
}
