using System;
using System.ComponentModel.DataAnnotations;

namespace Elisal.WasteManagement.Domain.Entities;

public class WasteType
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(20)]
    public string ColorCode { get; set; } = string.Empty; // CorIdentificacao

    public bool IsRecyclable { get; set; }
}
