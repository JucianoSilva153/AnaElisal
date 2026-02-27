using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Elisal.WasteManagement.Domain.Entities;

public class RouteExecution
{
    [Key] public int Id { get; set; }

    [Required] public int RouteId { get; set; }

    [ForeignKey(nameof(RouteId))]
    [JsonIgnore]
    public Route Route { get; set; } = null!;

    [Required] public int DriverId { get; set; }

    [ForeignKey(nameof(DriverId))]
    [JsonIgnore]
    public User Driver { get; set; } = null!;

    [Required] public DateTime StartTime { get; set; } = DateTime.UtcNow;

    public DateTime? EndTime { get; set; }

    public RouteExecutionStatus Status { get; set; } = RouteExecutionStatus.InProgress;

    public ICollection<RoutePointExecutionStatus> PointStatuses { get; set; } = new List<RoutePointExecutionStatus>();
}

public class RoutePointExecutionStatus
{
    [Key] public int Id { get; set; }

    [Required] public int RouteExecutionId { get; set; }

    [ForeignKey(nameof(RouteExecutionId))]
    [JsonIgnore]
    public RouteExecution RouteExecution { get; set; } = null!;

    [Required] public int CollectionPointId { get; set; }

    [ForeignKey(nameof(CollectionPointId))]
    public CollectionPoint CollectionPoint { get; set; } = null!;

    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedAt { get; set; }
}

public enum RouteExecutionStatus
{
    InProgress,
    Completed,
    Cancelled
}
