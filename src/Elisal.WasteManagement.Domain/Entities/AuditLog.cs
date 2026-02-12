using System;
using System.ComponentModel.DataAnnotations;

namespace Elisal.WasteManagement.Domain.Entities;

public class AuditLog
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Action { get; set; } = string.Empty; // acao

    [Required]
    [MaxLength(100)]
    public string TableName { get; set; } = string.Empty; // tabela_afetada

    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow; // data_hora

    [MaxLength(2000)]
    public string Details { get; set; } = string.Empty; // detalhes
}
