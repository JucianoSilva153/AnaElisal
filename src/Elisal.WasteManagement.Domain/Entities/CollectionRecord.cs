using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Elisal.WasteManagement.Domain.Entities;

public class CollectionRecord
{
    [Key] public int Id { get; set; }

    [Required] public DateTime DateTime { get; set; } = DateTime.UtcNow;

    public double? AmountKg { get; set; }

    [MaxLength(500)] public string Notes { get; set; } = string.Empty;

    public int? WasteTypeId { get; set; }

    [ForeignKey(nameof(WasteTypeId))] public WasteType? WasteType { get; set; }

    [Required] public int CollectionPointId { get; set; }

    [ForeignKey(nameof(CollectionPointId))]
    public CollectionPoint CollectionPoint { get; set; } = null!;

    [Required] public int UserId { get; set; }

    [ForeignKey(nameof(UserId))] public User User { get; set; } = null!;

    public ICollection<CollectionRecordWasteType> RecordWasteTypes { get; set; } =
        new List<CollectionRecordWasteType>();
}
