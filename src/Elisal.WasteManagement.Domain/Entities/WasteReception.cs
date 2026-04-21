using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Elisal.WasteManagement.Domain.Entities;

/// <summary>
/// Recepção de resíduos — registo de entrada de material no centro de triagem.
/// Liga-se opcionalmente a uma execução de rota (de onde veio o material).
/// </summary>
public class WasteReception
{
    [Key]
    public int Id { get; set; }

    [Required]
    public DateTime DateTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Peso bruto (veículo + carga) em Kg.
    /// </summary>
    [Required]
    public double GrossWeightKg { get; set; }

    /// <summary>
    /// Tara (peso do veículo vazio) em Kg.
    /// </summary>
    public double TareWeightKg { get; set; }

    /// <summary>
    /// Peso líquido (calculado: GrossWeightKg - TareWeightKg) em Kg.
    /// </summary>
    [NotMapped]
    public double NetWeightKg => GrossWeightKg - TareWeightKg;

    /// <summary>
    /// Peso líquido persistido na BD para queries eficientes.
    /// </summary>
    public double NetWeightKgStored { get; set; }

    /// <summary>
    /// Execução de rota de origem (opcional — pode ser recepção avulsa).
    /// </summary>
    public int? RouteExecutionId { get; set; }

    [ForeignKey(nameof(RouteExecutionId))]
    public RouteExecution? RouteExecution { get; set; }

    /// <summary>
    /// Centro de triagem onde o material foi recepcionado.
    /// </summary>
    [Required]
    public int SortingCenterId { get; set; }

    [ForeignKey(nameof(SortingCenterId))]
    public SortingCenter SortingCenter { get; set; } = null!;

    /// <summary>
    /// Utilizador que registou a recepção.
    /// </summary>
    [Required]
    public int ReceivedByUserId { get; set; }

    [ForeignKey(nameof(ReceivedByUserId))]
    public User ReceivedByUser { get; set; } = null!;

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Indica se já foi triado (tem lote de triagem associado).
    /// </summary>
    public bool IsSorted { get; set; } = false;

    public ICollection<SortingBatch> SortingBatches { get; set; } = new List<SortingBatch>();
}
