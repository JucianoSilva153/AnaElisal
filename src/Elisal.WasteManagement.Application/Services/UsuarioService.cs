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
    private readonly IEmailService _emailService;

    public UsuarioService(IUserRepository userRepository, IRepository<AuditLog> auditLogRepository,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _auditLogRepository = auditLogRepository;
        _emailService = emailService;
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
        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var existingUser = await _userRepository.GetByEmailAsync(normalizedEmail);
        if (existingUser != null)
            throw new InvalidOperationException("Email já cadastrado.");

        var usuario = new User
        {
            Name = dto.Nome,
            Email = normalizedEmail,
            PasswordHash = HashSenha(dto.Senha),
            Role = dto.Perfil,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _userRepository.AddAsync(usuario);
        await _userRepository.SaveChangesAsync();

        await LogActionAsync(usuario.Id, "Criar", "Usuario", $"Novo usuário criado: {usuario.Email}");

        // Envido de email com credenciais
        try
        {
            await _emailService.SendCredentialsEmailAsync(usuario.Email, usuario.Name, dto.Senha);
        }
        catch (Exception ex)
        {
            // Log do erro de email mas não impede a criação do usuário
            Console.WriteLine($"Erro ao enviar email de boas-vindas: {ex.Message}");
        }

        return usuario.ToDto();
    }

    public async Task AtualizarPerfilAsync(int id, UpdateUserDto dto)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new InvalidOperationException("Usuário não encontrado.");

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

        // Verificar se o email já existe em outro utilizador
        var existingUser = await _userRepository.GetByEmailAsync(normalizedEmail);
        if (existingUser != null && existingUser.Id != id)
            throw new InvalidOperationException("Este email já está a ser utilizado por outro utilizador.");

        user.Name = dto.Nome;
        user.Email = normalizedEmail;

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        await LogActionAsync(id, "Atualizar", "Usuario",
            $"Perfil atualizado. Novo Nome: {user.Name}, Novo Email: {user.Email}");
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

        // Envio de email com senha temporária
        try
        {
            await _emailService.SendResetPasswordEmailAsync(user.Email, user.Name, senhaTemporaria);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao enviar email de reset: {ex.Message}");
        }

        return senhaTemporaria;
    }

    public async Task AlterarSenhaAsync(int id, string senhaAtual, string novaSenha)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new InvalidOperationException("Usuário não encontrado.");

        if (!BCrypt.Net.BCrypt.Verify(senhaAtual, user.PasswordHash))
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
