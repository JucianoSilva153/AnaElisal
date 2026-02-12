using Elisal.WasteManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Elisal.WasteManagement.Domain.Enums;

namespace Elisal.WasteManagement.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Usuario");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.Name).HasColumnName("nome").IsRequired().HasMaxLength(150);
        builder.Property(e => e.Email).HasColumnName("email").IsRequired().HasMaxLength(150);
        builder.Property(e => e.PasswordHash).HasColumnName("senha_hash").IsRequired();
        builder.Property(e => e.Role)
            .HasColumnName("perfil")
            .HasMaxLength(50)
            .HasConversion<string>();
        builder.Property(e => e.IsActive).HasColumnName("ativo");
        builder.Property(e => e.CreatedAt).HasColumnName("data_cadastro");

        builder.HasData(new User
        {
            Id = 1,
            Name = "Root Admin",
            Email = "root@elisal.ao",
            PasswordHash = "$2a$12$kRntuZvjc1inEbVrN//K/uVjXSiPDTuX8eJtmIKa9fc9rlFs483/a", // Admin@123
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
