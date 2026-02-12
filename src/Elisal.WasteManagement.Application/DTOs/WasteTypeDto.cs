namespace Elisal.WasteManagement.Application.DTOs;

public class WasteTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ColorCode { get; set; } = string.Empty;
    public bool IsRecyclable { get; set; }
}
