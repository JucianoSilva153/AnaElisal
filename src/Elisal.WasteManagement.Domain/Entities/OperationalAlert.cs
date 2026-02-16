using System;

namespace Elisal.WasteManagement.Domain.Entities;

public class OperationalAlert
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "Info"; // Info, Warning, Critical
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; }
    public int? CollectionPointId { get; set; }
    public int? RouteId { get; set; }
}
