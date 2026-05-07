using System;
using System.ComponentModel.DataAnnotations;

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
    [Required(ErrorMessage = "O peso bruto é obrigatório")]
    [Range(0.1, 100000, ErrorMessage = "O peso bruto deve ser superior a 0")]
    public double GrossWeightKg { get; set; }

    [Required(ErrorMessage = "A tara é obrigatória")]
    [Range(0, 100000, ErrorMessage = "A tara não pode ser negativa")]
    public double TareWeightKg { get; set; }

    public int? RouteExecutionId { get; set; }

    [Required(ErrorMessage = "O centro de triagem é obrigatório")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecione um centro de triagem válido")]
    public int SortingCenterId { get; set; }

    public int ReceivedByUserId { get; set; }

    [MaxLength(500, ErrorMessage = "As observações não podem exceder 500 caracteres")]
    public string Notes { get; set; } = string.Empty;
}
