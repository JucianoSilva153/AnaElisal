using Elisal.WasteManagement.Application.DTOs;
using Elisal.WasteManagement.Domain.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Elisal.WasteManagement.Application.Interfaces;

public interface IUsuarioService
{
    Task<UserDto?> AutenticarAsync(string email, string senha);
    Task<UserDto> CriarUsuarioAsync(RegisterUserDto dto);
    Task AtualizarPerfilAsync(int id, UpdateUserDto dto);
    Task DesativarUsuarioAsync(int id);
    Task AtivarUsuarioAsync(int id);
    Task<string> GerarSenhaTemporariaAsync(int id);
    Task AlterarSenhaAsync(int id, string senhaAtual, string novaSenha);
    Task<IEnumerable<UserDto>> ObterPorPerfilAsync(UserRole perfil);
    string HashSenha(string senha);
}
