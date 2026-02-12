using System.ComponentModel.DataAnnotations;

namespace Elisal.WasteManagement.Application.DTOs;

public class UpdateUserDto
{
    [Required]
    [MaxLength(150)]
    public string Nome { get; set; } = string.Empty;
}
