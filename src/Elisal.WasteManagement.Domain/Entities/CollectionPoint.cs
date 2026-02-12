using System;
using System.ComponentModel.DataAnnotations;

namespace Elisal.WasteManagement.Domain.Entities;

public class CollectionPoint
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
    public double Capacity { get; set; }
    public double CurrentOccupancy { get; set; }
    public bool IsActive { get; set; } = true;

    [MaxLength(100)]
    public string Municipality { get; set; } = string.Empty;
}
