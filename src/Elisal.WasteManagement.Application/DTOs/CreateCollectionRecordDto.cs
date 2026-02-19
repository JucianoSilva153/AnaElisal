using System;

namespace Elisal.WasteManagement.Application.DTOs;

public class CreateCollectionRecordDto
{
    public DateTime DateTime { get; set; } = DateTime.Now;
    public double AmountKg { get; set; }
    public string Notes { get; set; } = string.Empty;
    public int WasteTypeId { get; set; }
    public List<int> WasteTypeIds { get; set; } = new();
    public int CollectionPointId { get; set; }
    public int UserId { get; set; }
}
