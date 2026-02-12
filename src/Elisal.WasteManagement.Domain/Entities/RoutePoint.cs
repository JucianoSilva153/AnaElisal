using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Elisal.WasteManagement.Domain.Entities;

public class RoutePoint
{
    [Required]
    public int RouteId { get; set; }

    [ForeignKey(nameof(RouteId))]
    public Route Route { get; set; } = null!;

    [Required]
    public int CollectionPointId { get; set; }

    [ForeignKey(nameof(CollectionPointId))]
    public CollectionPoint CollectionPoint { get; set; } = null!;

    [Required]
    public int SequenceOrder { get; set; }
}
