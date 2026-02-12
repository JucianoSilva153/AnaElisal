using Elisal.WasteManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Elisal.WasteManagement.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("LogAuditoria");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.UserId).HasColumnName("usuario_id");
        builder.Property(e => e.Action).HasColumnName("acao").HasMaxLength(50);
        builder.Property(e => e.TableName).HasColumnName("tabela_afetada").HasMaxLength(100);
        builder.Property(e => e.Timestamp).HasColumnName("data_hora");
        builder.Property(e => e.Details).HasColumnName("detalhes").HasMaxLength(2000);
    }
}
