using System.ComponentModel.DataAnnotations;
using Elisal.WasteManagement.Domain.Enums;

namespace Elisal.WasteManagement.Application.DTOs;

public class RegisterUserDto
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    [MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória")]
    [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres")]
    public string Senha { get; set; } = string.Empty;

    [Required(ErrorMessage = "O perfil é obrigatório")]
    public UserRole Perfil { get; set; }
}
