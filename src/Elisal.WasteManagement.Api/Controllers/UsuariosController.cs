using System;
using System.Threading.Tasks;
using Elisal.WasteManagement.Application.DTOs;
using Elisal.WasteManagement.Application.Interfaces;
using Elisal.WasteManagement.Domain.Entities;
using Elisal.WasteManagement.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Elisal.WasteManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;
    private readonly IUserRepository _userRepository;
    private readonly IRepository<AuditLog> _auditLogRepository;

    public UsuariosController(IUsuarioService usuarioService, IUserRepository userRepository,
        IRepository<AuditLog> auditLogRepository)
    {
        _usuarioService = usuarioService;
        _userRepository = userRepository;
        _auditLogRepository = auditLogRepository;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            var user = await _usuarioService.AutenticarAsync(loginDto.Email, loginDto.Senha);
            if (user == null)
                return Unauthorized(new { Message = "Email ou senha inválidos." });

            // In a real app, generate JWT Token here
            return Ok(new { Message = "Login realizado com sucesso", UserId = user.Id, Role = user.Role.ToString() });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Erro interno ao processar login", Details = ex.Message });
        }
    }

    [HttpPost("registar")]
    [AllowAnonymous]
    public async Task<IActionResult> Registar([FromBody] RegisterUserDto dto)
    {
        try
        {
            var createdUser = await _usuarioService.CriarUsuarioAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
        }
        catch (InvalidOperationException ex) // Business rule violation
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Erro ao registar usuário", Details = ex.Message });
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userRepository.GetAllAsync();
        var dtos = users.Select(u => new UserDto
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            Role = u.Role,
            IsActive = u.IsActive
        });
        return Ok(dtos);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return NotFound();

        var dto = new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive
        };
        return Ok(dto);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
    {
        try
        {
            // Allow user to update themselves OR allow Admin to update anyone
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && currentUserId != id.ToString())
            {
                return Forbid();
            }

            await _usuarioService.AtualizarPerfilAsync(id, dto);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Erro ao atualizar perfil", Details = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _usuarioService.DesativarUsuarioAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Erro ao desativar usuário", Details = ex.Message });
        }
    }

    [HttpPut("{id}/ativar")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Activate(int id)
    {
        try
        {
            await _usuarioService.AtivarUsuarioAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpPost("{id}/reset-senha")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ResetPassword(int id)
    {
        try
        {
            var senhaTemporaria = await _usuarioService.GerarSenhaTemporariaAsync(id);
            return Ok(new { Message = "Senha resetada com sucesso", SenhaTemporaria = senhaTemporaria });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpPost("{id}/alterar-senha")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto dto)
    {
        try
        {
            await _usuarioService.AlterarSenhaAsync(id, dto.SenhaAtual, dto.NovaSenha);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("{id}/historico")]
    [Authorize]
    public async Task<IActionResult> GetActivityHistory(int id)
    {
        var logs = (await _auditLogRepository.GetAllAsync())
            .Where(l => l.UserId == id)
            .OrderByDescending(l => l.Timestamp)
            .Take(50);

        return Ok(logs);
    }
}
