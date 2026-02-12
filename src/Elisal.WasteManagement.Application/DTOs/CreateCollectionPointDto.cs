using System.ComponentModel.DataAnnotations;

namespace Elisal.WasteManagement.Application.DTOs;

public class CreateCollectionPointDto
{
    [Required]
    [MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Endereco { get; set; } = string.Empty;

    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Capacidade { get; set; }
    
    [MaxLength(100)]
    public string Municipio { get; set; } = string.Empty;
}
