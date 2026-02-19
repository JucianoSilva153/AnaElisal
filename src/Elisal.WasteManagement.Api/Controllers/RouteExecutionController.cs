using Elisal.WasteManagement.Application.DTOs;
using Elisal.WasteManagement.Domain.Entities;
using Elisal.WasteManagement.Domain.Interfaces;
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
    private readonly IRepository<RouteExecution> _executionRepo;
    private readonly IRepository<RoutePointExecutionStatus> _pointStatusRepo;
    private readonly IRepository<Elisal.WasteManagement.Domain.Entities.Route> _routeRepo;
    private readonly Elisal.WasteManagement.Infrastructure.Persistence.ElisalDbContext _context;

    public RouteExecutionController(
        IRepository<RouteExecution> executionRepo,
        IRepository<RoutePointExecutionStatus> pointStatusRepo,
        IRepository<Elisal.WasteManagement.Domain.Entities.Route> routeRepo,
        Elisal.WasteManagement.Infrastructure.Persistence.ElisalDbContext context)
    {
        _executionRepo = executionRepo;
        _pointStatusRepo = pointStatusRepo;
        _routeRepo = routeRepo;
        _context = context;
    }

    [HttpPost("start/{routeId}")]
    public async Task<IActionResult> StartExecution(int routeId)
    {
        var route = await _routeRepo.GetByIdAsync(routeId);
        if (route == null) return NotFound("Rota não encontrada.");

        // Obter ID do motorista autenticado do JWT
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var driverId))
        {
            return Unauthorized("Usuário não identificado.");
        }

        var execution = new RouteExecution
        {
            RouteId = routeId,
            DriverId = driverId,
            StartTime = DateTime.UtcNow,
            Status = RouteExecutionStatus.InProgress
        };

        var created = await _executionRepo.AddAsync(execution);

        // Inicializar status dos pontos - Usando context diretamente para incluir RoutePoints
        var routeWithPoints = await _context.Routes
            .Include(r => r.RoutePoints)
            .FirstOrDefaultAsync(r => r.Id == routeId);

        if (routeWithPoints != null)
        {
            foreach (var rp in routeWithPoints.RoutePoints)
            {
                await _pointStatusRepo.AddAsync(new RoutePointExecutionStatus
                {
                    RouteExecutionId = execution.Id,
                    CollectionPointId = rp.CollectionPointId,
                    IsCompleted = false
                });
            }
        }

        return Ok(new RouteExecutionResponseDto { ExecutionId = execution.Id, Status = execution.ToDto() });
    }

    [HttpGet("active/{routeId}")]
    public async Task<IActionResult> GetActiveExecution(int routeId)
    {
        var execution = (await _executionRepo.GetAllAsync())
            .OrderByDescending(e => e.StartTime)
            .FirstOrDefault(e => e.RouteId == routeId && e.Status == RouteExecutionStatus.InProgress);

        if (execution == null) return NotFound();

        // Carregar status dos pontos com detalhes do CollectionPoint
        var points = await _context.RoutePointExecutionStatuses
            .Include(p => p.CollectionPoint)
            .Where(p => p.RouteExecutionId == execution.Id)
            .ToListAsync();

        return Ok(new { Execution = execution, Points = points });
    }

    [HttpPut("{executionId}/point")]
    public async Task<IActionResult> UpdatePointStatus(int executionId, [FromBody] UpdatePointStatusDto dto)
    {
        var status = (await _pointStatusRepo.GetAllAsync())
            .FirstOrDefault(s => s.RouteExecutionId == executionId && s.CollectionPointId == dto.CollectionPointId);

        if (status == null) return NotFound();

        status.IsCompleted = dto.IsCompleted;
        status.CompletedAt = dto.IsCompleted ? DateTime.UtcNow : null;

        await _pointStatusRepo.UpdateAsync(status);
        return NoContent();
    }

    [HttpPost("{executionId}/finish")]
    public async Task<IActionResult> FinishExecution(int executionId)
    {
        var execution = await _executionRepo.GetByIdAsync(executionId);
        if (execution == null) return NotFound();

        execution.EndTime = DateTime.UtcNow;
        execution.Status = RouteExecutionStatus.Completed;

        await _executionRepo.UpdateAsync(execution);
        return Ok(execution);
    }
}
