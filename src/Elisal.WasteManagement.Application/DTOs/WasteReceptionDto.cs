using System;

namespace Elisal.WasteManagement.Application.DTOs;

public class WasteReceptionDto
{
    public int Id { get; set; }
    public DateTime DateTime { get; set; }
    public double GrossWeightKg { get; set; }
    public double TareWeightKg { get; set; }
    public double NetWeightKg { get; set; }
    public int? RouteExecutionId { get; set; }
    public string? RouteExecutionName { get; set; }
    public int SortingCenterId { get; set; }
    public string SortingCenterName { get; set; } = string.Empty;
    public int ReceivedByUserId { get; set; }
    public string ReceivedByUserName { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public bool IsSorted { get; set; }
}

public class CreateWasteReceptionDto
{
    public double GrossWeightKg { get; set; }
    public double TareWeightKg { get; set; }
    public int? RouteExecutionId { get; set; }
    public int SortingCenterId { get; set; }
    public int ReceivedByUserId { get; set; }
    public string Notes { get; set; } = string.Empty;
}
