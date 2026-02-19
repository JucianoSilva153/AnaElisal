using System;
using System.Linq;
using System.Threading.Tasks;
using Elisal.WasteManagement.Application.Interfaces;
using Elisal.WasteManagement.Domain.Entities;
using Elisal.WasteManagement.Domain.Interfaces;
using Elisal.WasteManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Route = Elisal.WasteManagement.Domain.Entities.Route;

namespace Elisal.WasteManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RotasController : ControllerBase
{
    private readonly IRepository<Route> _routeRepo;
    private readonly IRepository<RoutePoint> _routePointRepo;
    private readonly IRotaService _rotaService;
    private readonly ElisalDbContext _context;

    public RotasController(
        IRepository<Route> routeRepo,
        IRepository<RoutePoint> routePointRepo,
        IRotaService rotaService,
        ElisalDbContext context)
    {
        _routeRepo = routeRepo;
        _routePointRepo = routePointRepo;
        _rotaService = rotaService;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var routes = await _context.Routes
            .Include(r => r.RoutePoints)
            .ThenInclude(rp => rp.CollectionPoint)
            .ToListAsync();
        var dtos = routes.Select(r => r.ToDto());
        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var route = await _context.Routes
            .Include(r => r.RoutePoints)
            .ThenInclude(rp => rp.CollectionPoint)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (route == null) return NotFound();

        return Ok(route.ToDto());
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRouteDto dto)
    {
        try
        {
            var route = new Elisal.WasteManagement.Domain.Entities.Route
            {
                Name = dto.Nome,
                Description = dto.Descricao,
                WeekDay = dto.DiaSemana,
                StartTime = dto.HorarioInicio,
                IsActive = true
            };

            await _routeRepo.AddAsync(route);
            await _routeRepo.SaveChangesAsync();

            // Add route points
            if (dto.PontoIds != null && dto.PontoIds.Any())
            {
                int order = 1;
                foreach (var pontoId in dto.PontoIds)
                {
                    var routePoint = new RoutePoint
                    {
                        RouteId = route.Id,
                        CollectionPointId = pontoId,
                        SequenceOrder = order++
                    };
                    await _routePointRepo.AddAsync(routePoint);
                }

                await _routePointRepo.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetById), new { id = route.Id }, route);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Erro ao criar rota", Details = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateRouteDto dto)
    {
        try
        {
            var route = await _routeRepo.GetByIdAsync(id);
            if (route == null) return NotFound();

            route.Name = dto.Nome;
            route.Description = dto.Descricao;
            route.WeekDay = dto.DiaSemana;
            route.StartTime = dto.HorarioInicio;

            _routeRepo.Update(route);
            await _routeRepo.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Erro ao atualizar rota", Details = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var route = await _routeRepo.GetByIdAsync(id);
            if (route == null) return NotFound();

            route.IsActive = false;
            _routeRepo.Update(route);
            await _routeRepo.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Erro ao eliminar rota", Details = ex.Message });
        }
    }

    [HttpPost("otimizar")]
    public async Task<IActionResult> OptimizeRoute([FromBody] int[] pontoIds)
    {
        try
        {
            var optimized = await _rotaService.OtimizarRotaAsync(pontoIds.ToList());
            var distance = await _rotaService.CalcularDistanciaTotal(optimized);

            return Ok(new
            {
                Pontos = optimized.Select(p => new { p.Id, p.Name, p.Latitude, p.Longitude }),
                DistanciaTotal = distance
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Erro ao otimizar rota", Details = ex.Message });
        }
    }

    [HttpGet("{id}/pontos")]
    public async Task<IActionResult> GetRoutePoints(int id)
    {
        var routePoints = await _context.RoutePoints
            .Include(rp => rp.CollectionPoint)
            .Where(rp => rp.RouteId == id)
            .OrderBy(rp => rp.SequenceOrder)
            .ToListAsync();

        var dtos = routePoints.Select(rp => rp.ToDto()).ToList();
        return Ok(dtos);
    }
}

public class CreateRouteDto
{
    public string Nome { get; set; } = "";
    public string Descricao { get; set; } = "";
    public string DiaSemana { get; set; } = "";
    public TimeSpan HorarioInicio { get; set; }
    public int[] PontoIds { get; set; } = Array.Empty<int>();
}
