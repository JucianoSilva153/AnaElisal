using System;

namespace Elisal.WasteManagement.Application.DTOs;

public class LocationUpdateDto
{
    public int RouteExecutionId { get; set; }
    public int DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
