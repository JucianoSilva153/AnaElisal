using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Elisal.WasteManagement.Application.DTOs;

public class CreateCollectionRecordDto
{
    [Required]
    public DateTime DateTime { get; set; } = DateTime.Now;

    public double? AmountKg { get; set; }

    [MaxLength(250)]
    public string Notes { get; set; } = string.Empty;

    public int? WasteTypeId { get; set; }

    public List<int> WasteTypeIds { get; set; } = new();

    [Required]
    public int CollectionPointId { get; set; }

    [Required]
    public int UserId { get; set; }
}
