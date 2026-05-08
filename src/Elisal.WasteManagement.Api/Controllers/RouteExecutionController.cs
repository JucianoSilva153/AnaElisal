using Elisal.WasteManagement.Application.DTOs;
using Elisal.WasteManagement.Domain.Entities;
using Elisal.WasteManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Elisal.WasteManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RouteExecutionController : ControllerBase
{
    private readonly ElisalDbContext _context;

    public RouteExecutionController(ElisalDbContext context)
    {
        _context = context;
    }

    [HttpPost("start/{routeId}")]
    [Authorize(Roles = "Driver")]
    public async Task<IActionResult> StartExecution(int routeId)
    {
        var route = await _context.Routes.FirstOrDefaultAsync(r => r.Id == routeId);
        if (route == null) return NotFound("Rota não encontrada.");


        // Obter ID do motorista autenticado do JWT
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var driverId))
        {
            return Unauthorized("Usuário não identificado.");
        }

        // Verificar se já existe uma execução em curso para esta rota — evitar duplicados
        var existingExecution = await _context.RouteExecutions
            .FirstOrDefaultAsync(e => e.RouteId == routeId && e.Status == RouteExecutionStatus.InProgress);

        if (existingExecution != null)
        {
            return Ok(new RouteExecutionResponseDto
                { ExecutionId = existingExecution.Id, Status = existingExecution.ToDto() });
        }

        var execution = new RouteExecution
        {
            RouteId = routeId,
            DriverId = driverId,
            StartTime = DateTime.UtcNow,
            Status = RouteExecutionStatus.InProgress
        };

        _context.RouteExecutions.Add(execution);
        await _context.SaveChangesAsync();

        // Carregar pontos da rota pela ordem correcta
        var routeWithPoints = await _context.Routes
            .Include(r => r.RoutePoints.OrderBy(rp => rp.SequenceOrder))
            .FirstOrDefaultAsync(r => r.Id == routeId);

        if (routeWithPoints != null && routeWithPoints.RoutePoints.Any())
        {
            foreach (var rp in routeWithPoints.RoutePoints.OrderBy(rp => rp.SequenceOrder))
            {
                _context.RoutePointExecutionStatuses.Add(new RoutePointExecutionStatus
                {
                    RouteExecutionId = execution.Id,
                    CollectionPointId = rp.CollectionPointId,
                    IsCompleted = false
                });
            }

            await _context.SaveChangesAsync();
        }

        return Ok(new RouteExecutionResponseDto { ExecutionId = execution.Id, Status = execution.ToDto() });
    }

    [HttpGet("active/{routeId}")]
    public async Task<IActionResult> GetActiveExecution(int routeId)
    {
        // Usar _context directamente com Include para garantir que os dados relacionados são carregados
        var execution = await _context.RouteExecutions
            .Where(e => e.RouteId == routeId && e.Status == RouteExecutionStatus.InProgress)
            .OrderByDescending(e => e.StartTime)
            .FirstOrDefaultAsync();

        if (execution == null) return NotFound();

        // Carregar status dos pontos com detalhes do CollectionPoint, pela ordem de criação
        var points = await _context.RoutePointExecutionStatuses
            .Include(p => p.CollectionPoint)
            .Where(p => p.RouteExecutionId == execution.Id)
            .OrderBy(p => p.Id)
            .ToListAsync();

        return Ok(new { Execution = execution, Points = points });
    }

    [HttpPut("{executionId}/point")]
    [Authorize(Roles = "Driver")]
    public async Task<IActionResult> UpdatePointStatus(int executionId, [FromBody] UpdatePointStatusDto dto)
    {
        var status = await _context.RoutePointExecutionStatuses
            .Include(s => s.CollectionPoint)
            .FirstOrDefaultAsync(s =>
                s.RouteExecutionId == executionId && s.CollectionPointId == dto.CollectionPointId);

        if (status == null) return NotFound();

        // Validação GPS para pontos inacessíveis
        if (dto.IsSkipped && !status.IsSkipped)
        {
            if (!dto.DriverLatitude.HasValue || !dto.DriverLongitude.HasValue)
            {
                return BadRequest("Localização do motorista é necessária para marcar ponto como inacessível.");
            }

            if (status.CollectionPoint == null) return BadRequest("Dados do ponto de recolha não encontrados.");

            var distance = HaversineDistance(
                dto.DriverLatitude.Value, dto.DriverLongitude.Value,
                status.CollectionPoint.Latitude, status.CollectionPoint.Longitude);

            if (distance > 50) // 50 metros
            {
                return BadRequest($"Está demasiado longe do ponto ({distance:F0}m). Deve estar a menos de 50m para o marcar como inacessível.");
            }
        }

        status.IsCompleted = dto.IsCompleted;
        status.CompletedAt = dto.IsCompleted ? DateTime.UtcNow : null;
        status.IsSkipped = dto.IsSkipped;
        status.SkipReason = dto.SkipReason;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    private double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000; // metros
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    [HttpPost("{executionId}/finish")]
    [Authorize(Roles = "Driver")]
    public async Task<IActionResult> FinishExecution(int executionId)
    {
        var execution = await _context.RouteExecutions
            .FirstOrDefaultAsync(e => e.Id == executionId);

        if (execution == null) return NotFound();

        execution.EndTime = DateTime.UtcNow;
        execution.Status = RouteExecutionStatus.Completed;

        await _context.SaveChangesAsync();

        return Ok(new { Message = "Rota finalizada com sucesso.", ExecutionId = executionId });
    }

    /// <summary>
    /// Retorna os routeIds que têm execuções em curso (InProgress).
    /// Usado pela listagem de rotas para mostrar badges.
    /// </summary>
    [HttpGet("active-routes")]
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] RouteExecutionStatus? status = null)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var currentUserId))
        {
            return Unauthorized();
        }

        var isAdminOrManager = User.IsInRole("Admin") || User.IsInRole("Manager");

        var query = _context.RouteExecutions
            .Include(e => e.Route)
            .Include(e => e.Driver)
            .Include(e => e.PointStatuses)
            .AsQueryable();

        // Se for motorista, vê apenas o que é dele
        if (!isAdminOrManager)
        {
            query = query.Where(e => e.DriverId == currentUserId);
        }

        if (status.HasValue)
        {
            query = query.Where(e => e.Status == status.Value);
        }

        var history = await query
            .OrderByDescending(e => e.StartTime)
            .Select(e => new RouteExecutionSummaryDto
            {
                Id = e.Id,
                RouteId = e.RouteId,
                RouteName = e.Route.Name,
                DriverId = e.DriverId,
                DriverName = e.Driver.Name,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                Status = e.Status,
                PointsCompleted = e.PointStatuses.Count(ps => ps.IsCompleted),
                PointsSkipped = e.PointStatuses.Count(ps => ps.IsSkipped),
                TotalPoints = e.PointStatuses.Count
            })
            .ToListAsync();

        return Ok(history);
    }
}
