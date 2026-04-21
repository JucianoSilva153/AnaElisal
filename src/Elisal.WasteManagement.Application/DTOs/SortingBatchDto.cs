using System;
using System.Collections.Generic;
using Elisal.WasteManagement.Domain.Enums;

namespace Elisal.WasteManagement.Application.DTOs;

public class SortingBatchDto
{
    public int Id { get; set; }
    public int WasteReceptionId { get; set; }
    public double ReceptionNetWeightKg { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
    public SortingBatchStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public int OperatorUserId { get; set; }
    public string OperatorName { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public double TotalOutputWeightKg { get; set; }
    public double ApprovalRate { get; set; } // % do peso que foi aproveitado (não rejeitado)
    public List<SortingBatchOutputDto> Outputs { get; set; } = new();
}

public class CreateSortingBatchDto
{
    public int WasteReceptionId { get; set; }
    public int OperatorUserId { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class SortingBatchOutputDto
{
    public int Id { get; set; }
    public int SortingBatchId { get; set; }
    public int WasteTypeId { get; set; }
    public string WasteTypeName { get; set; } = string.Empty;
    public string WasteTypeColor { get; set; } = string.Empty;
    public double WeightKg { get; set; }
    public QualityGrade QualityGrade { get; set; }
    public string QualityGradeText { get; set; } = string.Empty;
    public WasteDestinationType DestinationType { get; set; }
    public string DestinationTypeText { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

public class CreateSortingBatchOutputDto
{
    public int SortingBatchId { get; set; }
    public int WasteTypeId { get; set; }
    public double WeightKg { get; set; }
    public QualityGrade QualityGrade { get; set; }
    public WasteDestinationType DestinationType { get; set; }
    public string Notes { get; set; } = string.Empty;
}

/// <summary>
/// Indicadores ambientais do módulo GRS.
/// </summary>
public class EnvironmentalIndicatorsDto
{
    public double TotalReceivedTons { get; set; }
    public double TotalSortedTons { get; set; }
    public double RecyclingTons { get; set; }
    public double CompostingTons { get; set; }
    public double LandfillTons { get; set; }
    public double IncinerationTons { get; set; }
    public double InternalReuseTons { get; set; }
    public double HazardousDisposalTons { get; set; }
    public double LandfillDiversionRate { get; set; } // % desviado do aterro
    public double Co2AvoidedTons { get; set; } // CO₂ evitado
    public double OverallApprovalRate { get; set; } // % de aproveitamento global
    public int PendingReceptions { get; set; } // Recepções sem triagem
    public List<DestinationBreakdownDto> DestinationBreakdown { get; set; } = new();
    public List<ChartSeriesDto> MonthlyTrend { get; set; } = new();
}

public class DestinationBreakdownDto
{
    public string Destination { get; set; } = string.Empty;
    public double Percentage { get; set; }
    public double WeightTons { get; set; }
    public string Color { get; set; } = string.Empty;
}
