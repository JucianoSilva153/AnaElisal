namespace Elisal.WasteManagement.Application.DTOs;

public class UpdatePointStatusDto
{
    public int CollectionPointId { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsSkipped { get; set; }
    public string? SkipReason { get; set; }
    public double? DriverLatitude { get; set; }
    public double? DriverLongitude { get; set; }
}
