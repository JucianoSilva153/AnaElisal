using System;

namespace Elisal.WasteManagement.Application.DTOs;

public class OperationalAlertDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "Info";
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public int? CollectionPointId { get; set; }
    public int? RouteId { get; set; }
}
