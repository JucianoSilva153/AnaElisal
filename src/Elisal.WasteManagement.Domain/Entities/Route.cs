using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Elisal.WasteManagement.Domain.Entities;

public class Route
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string WeekDay { get; set; } = string.Empty; // DiaSemana

    [Required]
    public TimeSpan StartTime { get; set; }

    public bool IsActive { get; set; } = true;
    public double TotalDistance { get; set; }

    public ICollection<RoutePoint> RoutePoints { get; set; } = new List<RoutePoint>();
}
