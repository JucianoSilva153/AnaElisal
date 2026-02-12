namespace Elisal.WasteManagement.Application.DTOs;

public class CooperativeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string AcceptedWasteTypes { get; set; } = string.Empty;
}
