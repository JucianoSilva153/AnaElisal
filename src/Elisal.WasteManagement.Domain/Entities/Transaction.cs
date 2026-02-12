using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Elisal.WasteManagement.Domain.Enums;

namespace Elisal.WasteManagement.Domain.Entities;

public class Transaction
{
    [Key]
    public int Id { get; set; }

    [Required]
    public DateTime Date { get; set; } = DateTime.UtcNow;

    [Required]
    public double AmountKg { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Value { get; set; }

    [Required]
    public TransactionStatus Status { get; set; }

    [Required]
    public int WasteTypeId { get; set; }

    [ForeignKey(nameof(WasteTypeId))]
    public WasteType WasteType { get; set; } = null!;

    [Required]
    public int CooperativeId { get; set; }

    [ForeignKey(nameof(CooperativeId))]
    public Cooperative Cooperative { get; set; } = null!;
}
