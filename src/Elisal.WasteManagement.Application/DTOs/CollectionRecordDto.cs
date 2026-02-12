using System;

namespace Elisal.WasteManagement.Application.DTOs;

public class CollectionRecordDto
{
    public int Id { get; set; }
    public DateTime DateTime { get; set; }
    public double AmountKg { get; set; }
    public string Notes { get; set; } = string.Empty;
    public int WasteTypeId { get; set; }
    public string WasteTypeName { get; set; } = string.Empty;
    public int CollectionPointId { get; set; }
    public string CollectionPointName { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
}
