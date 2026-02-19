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
[Authorize]
public class ResiduosController : ControllerBase
{
    private readonly IResiduoService _residuoService;
    private readonly IRepository<WasteType> _wasteTypeRepository;

    public ResiduosController(IResiduoService residuoService, IRepository<WasteType> wasteTypeRepository)
    {
        _residuoService = residuoService;
        _wasteTypeRepository = wasteTypeRepository;
    }

    [HttpPost("recolha")]
    public async Task<IActionResult> RegistarRecolha([FromBody] CreateCollectionRecordDto dto)
    {
        try
        {
            var result = await _residuoService.RegistarRecolhaAsync(dto);
            return CreatedAtAction(nameof(RegistarRecolha), null, result); // Usually GetById but simplifying
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Erro ao registar recolha", Details = ex.Message });
        }
    }

    [HttpGet("tipos")]
    public async Task<IActionResult> GetTipos()
    {
        var types = await _wasteTypeRepository.GetAllAsync();
        var dtos = types.Select(t => new WasteTypeDto
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            ColorCode = t.ColorCode,
            IsRecyclable = t.IsRecyclable
        });
        return Ok(dtos);
    }

    [HttpPost("tipos")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CriarTipo([FromBody] WasteTypeDto dto)
    {
        var type = new WasteType
        {
            Name = dto.Name,
            Description = dto.Description,
            ColorCode = dto.ColorCode,
            IsRecyclable = dto.IsRecyclable
        };
        await _wasteTypeRepository.AddAsync(type);
        await _wasteTypeRepository.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTipos), new { id = type.Id }, type);
    }

    [HttpPut("tipos/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> AtualizarTipo(int id, [FromBody] WasteTypeDto dto)
    {
        if (id != dto.Id) return BadRequest();

        var type = await _wasteTypeRepository.GetByIdAsync(id);
        if (type == null) return NotFound();

        type.Name = dto.Name;
        type.Description = dto.Description;
        type.ColorCode = dto.ColorCode;
        type.IsRecyclable = dto.IsRecyclable;

        await _wasteTypeRepository.UpdateAsync(type);
        return NoContent();
    }

    [HttpDelete("tipos/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeletarTipo(int id)
    {
        var type = await _wasteTypeRepository.GetByIdAsync(id);
        if (type == null) return NotFound();
        await _wasteTypeRepository.DeleteAsync(type);
        return NoContent();
    }

    [HttpGet("recolhas")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetRecolhas([FromQuery] DateTime? dataInicio, [FromQuery] DateTime? dataFim)
    {
        if (!dataInicio.HasValue || !dataFim.HasValue)
            return BadRequest(new { Message = "Data de Início e Fim são obrigatórias." });

        // Ajustar data fim para incluir o dia inteiro
        var fimAjustado = dataFim.Value.Date.AddDays(1).AddTicks(-1);
        var results = await _residuoService.ObterEstatisticasPorPeriodoAsync(dataInicio.Value.Date, fimAjustado);
        return Ok(results);
    }

    [HttpGet("estatisticas")]
    public async Task<IActionResult> GetEstatisticas([FromQuery] DateTime? periodo) // Simulação period approx
    {
        if (!periodo.HasValue)
            return BadRequest(new { Message = "Período obrigatório." });

        // Simulating monthly stats based on the provided date's month
        var start = new DateTime(periodo.Value.Year, periodo.Value.Month, 1);
        var end = start.AddMonths(1).AddDays(-1);

        var recyclingRate = await _residuoService.CalcularTaxaReciclagemAsync(start, end);

        return Ok(new
        {
            Mes = start.ToString("MMMM/yyyy"),
            TaxaReciclagem = recyclingRate
        });
    }
}
