using Elisal.WasteManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Elisal.WasteManagement.Infrastructure.Persistence.Configurations;

public class CooperativeConfiguration : IEntityTypeConfiguration<Cooperative>
{
    public void Configure(EntityTypeBuilder<Cooperative> builder)
    {
        builder.ToTable("Cooperativa");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.Name).HasColumnName("nome").IsRequired().HasMaxLength(150);
        builder.Property(e => e.Contact).HasColumnName("contacto").HasMaxLength(50);
        builder.Property(e => e.Email).HasColumnName("email").HasMaxLength(150);
        builder.Property(e => e.Address).HasColumnName("endereco").HasMaxLength(255);
        builder.Property(e => e.AcceptedWasteTypes).HasColumnName("tipos_residuos_aceites").HasMaxLength(500);
    }
}
