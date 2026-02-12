using Elisal.WasteManagement.Application.DTOs;

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
    string HashSenha(string senha);
}
