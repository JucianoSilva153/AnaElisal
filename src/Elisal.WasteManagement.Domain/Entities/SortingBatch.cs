using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Elisal.WasteManagement.Domain.Enums;

namespace Elisal.WasteManagement.Domain.Entities;

/// <summary>
/// Lote de Triagem — registo do processo de separação de uma recepção de resíduos.
/// </summary>
public class SortingBatch
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int WasteReceptionId { get; set; }

    [ForeignKey(nameof(WasteReceptionId))]
    public WasteReception WasteReception { get; set; } = null!;

    [Required]
    public DateTime StartDateTime { get; set; } = DateTime.UtcNow;

    public DateTime? EndDateTime { get; set; }

    [Required]
    public SortingBatchStatus Status { get; set; } = SortingBatchStatus.Pending;

    /// <summary>
    /// Operador responsável pela triagem.
    /// </summary>
    [Required]
    public int OperatorUserId { get; set; }

    [ForeignKey(nameof(OperatorUserId))]
    public User Operator { get; set; } = null!;

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Outputs da triagem — cada material separado com tipo, peso, qualidade e destino.
    /// </summary>
    public ICollection<SortingBatchOutput> Outputs { get; set; } = new List<SortingBatchOutput>();
}
