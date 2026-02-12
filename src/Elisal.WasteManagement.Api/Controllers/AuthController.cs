using System;
using System.Threading.Tasks;
using Elisal.WasteManagement.Application.DTOs;
using Elisal.WasteManagement.Application.Interfaces;
using Elisal.WasteManagement.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Elisal.WasteManagement.Domain.Entities;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Elisal.WasteManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;
    private readonly ITokenService _tokenService;
    private readonly IRepository<UserToken> _tokenRepository;
    private readonly IUserRepository _userRepository;

    public AuthController(IUsuarioService usuarioService, ITokenService tokenService, IRepository<UserToken> tokenRepository, IUserRepository userRepository)
    {
        _usuarioService = usuarioService;
        _tokenService = tokenService;
        _tokenRepository = tokenRepository;
        _userRepository = userRepository;
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

            var token = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            var userToken = new UserToken
            {
                UserId = user.Id,
                Token = token,
                RefreshToken = refreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(7)
            };

            await _tokenRepository.AddAsync(userToken);
            await _tokenRepository.SaveChangesAsync();
            
            return Ok(new 
            { 
                Token = token,
                RefreshToken = refreshToken,
                User = new 
                { 
                    user.Id, 
                    user.Name, 
                    user.Email, 
                    Role = user.Role.ToString() 
                } 
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Erro no login", Details = ex.Message });
        }
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var allTokens = await _tokenRepository.GetAllAsync();
        var storedToken = allTokens.FirstOrDefault(t => t.Token == request.Token && t.RefreshToken == request.RefreshToken && !t.IsRevoked);

        if (storedToken == null || storedToken.ExpiryDate < DateTime.UtcNow)
            return Unauthorized(new { Message = "Token inválido ou expirado." });

        var user = await _userRepository.GetByIdAsync(storedToken.UserId);
        if (user == null) return Unauthorized();

        var userDto = user.ToDto();
        var newToken = _tokenService.GenerateAccessToken(userDto);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        storedToken.IsRevoked = true;
        _tokenRepository.Update(storedToken);

        var newUserToken = new UserToken
        {
            UserId = user.Id,
            Token = newToken,
            RefreshToken = newRefreshToken,
            ExpiryDate = DateTime.UtcNow.AddDays(7)
        };

        await _tokenRepository.AddAsync(newUserToken);
        await _tokenRepository.SaveChangesAsync();

        return Ok(new { Token = newToken, RefreshToken = newRefreshToken });
    }

    public class RefreshTokenRequest 
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        // Stateless JWT logout is usually client-side (delete token).
        return NoContent();
    }
}
