using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Elisal.WasteManagement.Domain.Enums;

namespace Elisal.WasteManagement.Domain.Entities;

/// <summary>
/// Output da triagem — cada linha representa um material separado,
/// com tipo, peso, classificação de qualidade e destino final.
/// </summary>
public class SortingBatchOutput
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int SortingBatchId { get; set; }

    [ForeignKey(nameof(SortingBatchId))]
    public SortingBatch SortingBatch { get; set; } = null!;

    /// <summary>
    /// Tipo de resíduo separado (liga à entidade WasteType existente).
    /// </summary>
    [Required]
    public int WasteTypeId { get; set; }

    [ForeignKey(nameof(WasteTypeId))]
    public WasteType WasteType { get; set; } = null!;

    /// <summary>
    /// Peso do material separado em Kg.
    /// </summary>
    [Required]
    public double WeightKg { get; set; }

    /// <summary>
    /// Classificação de qualidade do material.
    /// </summary>
    [Required]
    public QualityGrade QualityGrade { get; set; }

    /// <summary>
    /// Destino final do material.
    /// </summary>
    [Required]
    public WasteDestinationType DestinationType { get; set; }

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;
}
