using System;
using System.Threading.Tasks;
using Elisal.WasteManagement.Application.DTOs;
using Elisal.WasteManagement.Application.Interfaces;
using Elisal.WasteManagement.Domain.Entities;
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
public class PontosRecolhaController : ControllerBase
{
    private readonly IPontoRecolhaService _pontoRecolhaService;
    private readonly ICollectionPointRepository _repository;

    public PontosRecolhaController(IPontoRecolhaService pontoRecolhaService, ICollectionPointRepository repository)
    {
        _pontoRecolhaService = pontoRecolhaService;
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var pontos = await _repository.GetAllAsync();
        var dtos = pontos.Select(p => new CollectionPointDto
        {
            Id = p.Id,
            Name = p.Name,
            Address = p.Address,
            Latitude = p.Latitude,
            Longitude = p.Longitude,
            Capacity = p.Capacity,
            CurrentOccupancy = p.CurrentOccupancy,
            IsActive = p.IsActive,
            Municipality = p.Municipality
        });
        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var ponto = await _repository.GetByIdAsync(id);
        if (ponto == null) return NotFound();
        
        var dto = new CollectionPointDto
        {
            Id = ponto.Id,
            Name = ponto.Name,
            Address = ponto.Address,
            Latitude = ponto.Latitude,
            Longitude = ponto.Longitude,
            Capacity = ponto.Capacity,
            CurrentOccupancy = ponto.CurrentOccupancy,
            IsActive = ponto.IsActive,
            Municipality = ponto.Municipality
        };
        return Ok(dto);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] CreateCollectionPointDto dto)
    {
        try
        {
            var created = await _pontoRecolhaService.CriarPontoAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Erro ao criar ponto de recolha." });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateCapacity(int id, [FromBody] double novaCapacidade)
    {
        try
        {
            await _pontoRecolhaService.AtualizarCapacidadeAsync(id, novaCapacidade);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Erro ao atualizar capacidade." });
        }
    }

    [HttpGet("proximos")]
    public async Task<IActionResult> GetProximos([FromQuery] double lat, [FromQuery] double lng, [FromQuery] double raio)
    {
        var pontos = await _pontoRecolhaService.ObterPontosProximosAsync(lat, lng, raio);
        return Ok(pontos);
    }
}
