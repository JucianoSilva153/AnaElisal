using System;
using System.Threading.Tasks;
using Elisal.WasteManagement.Application.DTOs;
using Elisal.WasteManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Elisal.WasteManagement.Api.Controllers;

[ApiController]
[Route("api/gestao-residuos")]
[Authorize]
public class GestaoResiduosController : ControllerBase
{
    private readonly IWasteManagementService _service;

    public GestaoResiduosController(IWasteManagementService service)
    {
        _service = service;
    }

    private int GetCurrentUserId()
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdStr, out var id) ? id : 0;
    }

    #region Centros de Triagem

    [HttpGet("centros-triagem")]
    public async Task<IActionResult> GetCentros()
    {
        var result = await _service.GetAllCentrosAsync();
        return Ok(result);
    }

    [HttpPost("centros-triagem")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CriarCentro([FromBody] CreateSortingCenterDto dto)
    {
        try
        {
            var result = await _service.CriarCentroAsync(dto);
            return CreatedAtAction(nameof(GetCentros), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("centros-triagem/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> AtualizarCentro(int id, [FromBody] CreateSortingCenterDto dto)
    {
        var result = await _service.AtualizarCentroAsync(id, dto);
        if (result == null) return NotFound();
        return Ok(result);
    }

    #endregion

    #region Recepções

    [HttpPost("recepcoes")]
    [Authorize(Roles = "Admin,Manager,Driver")]
    public async Task<IActionResult> RegistarRecepcao([FromBody] CreateWasteReceptionDto dto)
    {
        try
        {
            dto.ReceivedByUserId = GetCurrentUserId();
            var result = await _service.RegistarRecepcaoAsync(dto);
            return CreatedAtAction(nameof(GetRecepcaoPorId), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("recepcoes")]
    public async Task<IActionResult> GetRecepcoes([FromQuery] DateTime? inicio, [FromQuery] DateTime? fim)
    {
        if (!inicio.HasValue || !fim.HasValue)
        {
            // Default: último mês
            var now = DateTime.UtcNow;
            inicio = new DateTime(now.Year, now.Month, 1);
            fim = now;
        }

        var result = await _service.GetRecepcoesPorPeriodoAsync(inicio.Value, fim.Value.Date.AddDays(1).AddTicks(-1));
        return Ok(result);
    }

    [HttpGet("recepcoes/pendentes")]
    public async Task<IActionResult> GetRecepcoesPendentes()
    {
        var result = await _service.GetRecepcoesPendentesAsync();
        return Ok(result);
    }

    [HttpGet("recepcoes/{id}")]
    public async Task<IActionResult> GetRecepcaoPorId(int id)
    {
        var result = await _service.GetRecepcaoByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    #endregion

    #region Triagens

    [HttpPost("triagens")]
    [Authorize(Roles = "Admin,Manager,Driver")]
    public async Task<IActionResult> IniciarTriagem([FromBody] CreateSortingBatchDto dto)
    {
        try
        {
            dto.OperatorUserId = GetCurrentUserId();
            var result = await _service.IniciarTriagemAsync(dto);
            return CreatedAtAction(nameof(GetTriagemPorId), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("triagens/{id}/outputs")]
    [Authorize(Roles = "Admin,Manager,Driver")]
    public async Task<IActionResult> RegistarOutput(int id, [FromBody] CreateSortingBatchOutputDto dto)
    {
        try
        {
            dto.SortingBatchId = id;
            var result = await _service.RegistarOutputTriagemAsync(dto);
            return CreatedAtAction(nameof(GetTriagemPorId), new { id = id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("triagens/{id}/concluir")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ConcluirTriagem(int id)
    {
        try
        {
            var result = await _service.ConcluirTriagemAsync(id);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("triagens")]
    public async Task<IActionResult> GetTriagens([FromQuery] DateTime? inicio, [FromQuery] DateTime? fim)
    {
        if (!inicio.HasValue || !fim.HasValue)
        {
            var now = DateTime.UtcNow;
            inicio = new DateTime(now.Year, now.Month, 1);
            fim = now;
        }

        var result = await _service.GetTriagensPorPeriodoAsync(inicio.Value, fim.Value.Date.AddDays(1).AddTicks(-1));
        return Ok(result);
    }

    [HttpGet("triagens/{id}")]
    public async Task<IActionResult> GetTriagemPorId(int id)
    {
        var result = await _service.GetTriagemByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    #endregion

    #region Indicadores Ambientais

    [HttpGet("indicadores")]
    public async Task<IActionResult> GetIndicadores([FromQuery] DateTime? inicio, [FromQuery] DateTime? fim)
    {
        if (!inicio.HasValue || !fim.HasValue)
        {
            var now = DateTime.UtcNow;
            inicio = now.AddMonths(-6);
            fim = now;
        }

        var result = await _service.GetIndicadoresAmbientaisAsync(inicio.Value, fim.Value.Date.AddDays(1).AddTicks(-1));
        return Ok(result);
    }

    [HttpGet("rotas-aguardando")]
    public async Task<IActionResult> GetRotasAguardando()
    {
        var result = await _service.GetRotasAguardandoRecepcaoAsync();
        return Ok(result);
    }

    #endregion
}
