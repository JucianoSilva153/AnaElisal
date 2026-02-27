using Elisal.WasteManagement.Domain.Entities;
using System;

namespace Elisal.WasteManagement.Application.DTOs;

public class RouteExecutionSummaryDto
{
    public int Id { get; set; }
    public int RouteId { get; set; }
    public string RouteName { get; set; } = string.Empty;
    public int DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public RouteExecutionStatus Status { get; set; }
    public int PointsCompleted { get; set; }
    public int TotalPoints { get; set; }
    
    public double PercentageComplete => TotalPoints == 0 ? 0 : Math.Round((double)PointsCompleted / TotalPoints * 100, 1);
    
    public string Duration
    {
        get
        {
            var end = EndTime ?? DateTime.UtcNow;
            var diff = end - StartTime;
            if (diff.TotalHours >= 1)
                return $"{(int)diff.TotalHours}h {diff.Minutes}m";
            return $"{diff.Minutes}m";
        }
    }
}
