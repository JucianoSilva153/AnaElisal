using System;
using System.ComponentModel.DataAnnotations;

namespace Elisal.WasteManagement.Domain.Entities;

/// <summary>
/// Centro de Triagem — local físico onde os resíduos recolhidos são recepcionados e processados.
/// </summary>
public class SortingCenter
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Address { get; set; } = string.Empty;

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    /// <summary>
    /// Capacidade máxima de processamento em toneladas/dia.
    /// </summary>
    public double CapacityTonsPerDay { get; set; }

    [MaxLength(100)]
    public string Municipality { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Contact { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
