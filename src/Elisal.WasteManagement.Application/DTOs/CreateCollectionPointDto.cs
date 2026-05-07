using System.ComponentModel.DataAnnotations;

namespace Elisal.WasteManagement.Application.DTOs;

public class CreateCollectionPointDto
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    [MaxLength(150, ErrorMessage = "O nome não pode exceder 150 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(255, ErrorMessage = "O endereço não pode exceder 255 caracteres")]
    public string Endereco { get; set; } = string.Empty;

    [Required]
    [Range(-90, 90, ErrorMessage = "Latitude inválida")]
    public double Latitude { get; set; }

    [Required]
    [Range(-180, 180, ErrorMessage = "Longitude inválida")]
    public double Longitude { get; set; }

    [Required(ErrorMessage = "A capacidade é obrigatória")]
    [Range(0.1, 100000, ErrorMessage = "A capacidade deve ser superior a 0")]
    public double Capacidade { get; set; }
    
    [MaxLength(100)]
    public string Municipio { get; set; } = string.Empty;
}
