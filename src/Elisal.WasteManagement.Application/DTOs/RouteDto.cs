using System;
using System.Collections.Generic;

namespace Elisal.WasteManagement.Application.DTOs;

public class RouteDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string WeekDay { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public bool IsActive { get; set; }
    public double TotalDistance { get; set; }
    public List<RoutePointDto> RoutePoints { get; set; } = new();
}

public class RoutePointDto
{
    public int RouteId { get; set; }
    public int CollectionPointId { get; set; }
    public int SequenceOrder { get; set; }
    public CollectionPointDto? CollectionPoint { get; set; }
}
