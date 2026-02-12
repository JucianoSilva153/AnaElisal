using System;
using System.Linq;
using BCrypt.Net;
using System.Text;
using System.Threading.Tasks;
using Elisal.WasteManagement.Application.Interfaces;
using Elisal.WasteManagement.Domain.Entities;
using Elisal.WasteManagement.Domain.Interfaces;
using Elisal.WasteManagement.Application.DTOs;

namespace Elisal.WasteManagement.Application.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUserRepository _userRepository;
    private readonly IRepository<AuditLog> _auditLogRepository;

    public UsuarioService(IUserRepository userRepository, IRepository<AuditLog> auditLogRepository)
    {
        _userRepository = userRepository;
        _auditLogRepository = auditLogRepository;
    }

    public async Task<UserDto?> AutenticarAsync(string email, string senha)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null || !user.IsActive)
            return null;

        if (!BCrypt.Net.BCrypt.Verify(senha, user.PasswordHash))
            return null;

        await LogActionAsync(user.Id, "Login", "Usuario", $"Login realizado com sucesso.");
        return user.ToDto();
    }

    public async Task<UserDto> CriarUsuarioAsync(RegisterUserDto dto)
    {
        var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
        if (existingUser != null)
            throw new InvalidOperationException("Email já cadastrado.");

        var usuario = new User
        {
            Name = dto.Nome,
            Email = dto.Email,
            PasswordHash = HashSenha(dto.Senha),
            Role = dto.Perfil,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _userRepository.AddAsync(usuario);
        await _userRepository.SaveChangesAsync();

        await LogActionAsync(usuario.Id, "Criar", "Usuario", $"Novo usuário criado: {usuario.Email}");

        return usuario.ToDto();
    }

    public async Task AtualizarPerfilAsync(int id, UpdateUserDto dto)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new InvalidOperationException("Usuário não encontrado.");

        user.Name = dto.Nome;
        
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        await LogActionAsync(id, "Atualizar", "Usuario", "Perfil atualizado.");
    }

    public async Task DesativarUsuarioAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new InvalidOperationException("Usuário não encontrado.");

        user.IsActive = false;
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        await LogActionAsync(id, "Desativar", "Usuario", "Usuário desativado.");
    }

    public async Task AtivarUsuarioAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new InvalidOperationException("Usuário não encontrado.");

        user.IsActive = true;
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        await LogActionAsync(id, "Ativar", "Usuario", "Usuário ativado.");
    }

    public async Task<string> GerarSenhaTemporariaAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new InvalidOperationException("Usuário não encontrado.");

        var senhaTemporaria = GerarSenhaAleatoria();
        user.PasswordHash = HashSenha(senhaTemporaria);
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        await LogActionAsync(id, "ResetSenha", "Usuario", "Senha resetada.");
        return senhaTemporaria;
    }

    public async Task AlterarSenhaAsync(int id, string senhaAtual, string novaSenha)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new InvalidOperationException("Usuário não encontrado.");

        if (user.PasswordHash != HashSenha(senhaAtual))
            throw new InvalidOperationException("Senha atual incorreta.");

        user.PasswordHash = HashSenha(novaSenha);
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        await LogActionAsync(id, "AlterarSenha", "Usuario", "Senha alterada.");
    }

    private string GerarSenhaAleatoria()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public string HashSenha(string senha)
    {
        return BCrypt.Net.BCrypt.HashPassword(senha, 12);
    }

    private async Task LogActionAsync(int userId, string action, string table, string details)
    {
        try 
        {
            var audit = new AuditLog
            {
                UserId = userId,
                Action = action,
                TableName = table,
                Timestamp = DateTime.UtcNow,
                Details = details
            };
            await _auditLogRepository.AddAsync(audit);
            await _auditLogRepository.SaveChangesAsync();
        }
        catch
        {
            // Fail silently or log to file/console if audit fails to prevent blocking business flow
        }
    }
}
