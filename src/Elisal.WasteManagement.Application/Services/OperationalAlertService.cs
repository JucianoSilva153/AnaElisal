using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elisal.WasteManagement.Application.DTOs;
using Elisal.WasteManagement.Application.Interfaces;
using Elisal.WasteManagement.Domain.Entities;
using Elisal.WasteManagement.Domain.Interfaces;

namespace Elisal.WasteManagement.Application.Services;

public class OperationalAlertService : IOperationalAlertService
{
    private readonly IOperationalAlertRepository _alertRepository;
    private readonly IRepository<CollectionPoint> _pointRepository;
    private readonly IRepository<RouteExecution> _routeExecutionRepository;

    public OperationalAlertService(
        IOperationalAlertRepository alertRepository,
        IRepository<CollectionPoint> pointRepository,
        IRepository<RouteExecution> routeExecutionRepository)
    {
        _alertRepository = alertRepository;
        _pointRepository = pointRepository;
        _routeExecutionRepository = routeExecutionRepository;
    }

    public async Task<IEnumerable<OperationalAlertDto>> GetActiveAlertsAsync()
    {
        var alerts = await _alertRepository.GetAllAsync();
        return alerts.Where(a => !a.IsRead)
                     .OrderByDescending(a => a.CreatedAt)
                     .Select(a => a.ToDto());
    }

    public async Task MarkAsReadAsync(int id)
    {
        var alert = await _alertRepository.GetByIdAsync(id);
        if (alert != null)
        {
            alert.IsRead = true;
            _alertRepository.Update(alert);
            await _alertRepository.SaveChangesAsync();
        }
    }

    public async Task ProcessAutomaticAlertsAsync()
    {
        await CheckFullPointsAsync();
        await CheckDelayedRoutesAsync();
    }

    private async Task CheckFullPointsAsync()
    {
        var points = await _pointRepository.GetAllAsync();
        // Threshold of 80%
        var fullPoints = points.Where(p => p.Capacity > 0 && (p.CurrentOccupancy / p.Capacity) >= 0.8);

        foreach (var point in fullPoints)
        {
            var alerts = await _alertRepository.GetAllAsync();
            var existingAlert = alerts.Any(a => a.CollectionPointId == point.Id && !a.IsRead && a.CreatedAt.Date == DateTime.UtcNow.Date);

            if (!existingAlert)
            {
                await _alertRepository.AddAsync(new OperationalAlert
                {
                    Title = "Capacidade Crítica",
                    Message = $"O ponto de recolha '{point.Name}' atingiu {(point.CurrentOccupancy / point.Capacity * 100):F0}% da capacidade.",
                    Type = "Critical",
                    CollectionPointId = point.Id,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
        await _alertRepository.SaveChangesAsync();
    }

    private async Task CheckDelayedRoutesAsync()
    {
        var executions = await _routeExecutionRepository.GetAllAsync();
        // Threshold of 4 hours
        var delayedExecutions = executions.Where(e => e.Status == RouteExecutionStatus.InProgress 
                                                   && (DateTime.UtcNow - e.StartTime).TotalHours > 4);

        foreach (var execution in delayedExecutions)
        {
            var alerts = await _alertRepository.GetAllAsync();
            var existingAlert = alerts.Any(a => a.RouteId == execution.RouteId && !a.IsRead && a.CreatedAt.Date == DateTime.UtcNow.Date);

            if (!existingAlert)
            {
                await _alertRepository.AddAsync(new OperationalAlert
                {
                    Title = "Rota Atrasada",
                    Message = $"A execução da rota (ID {execution.RouteId}) está em curso há mais de 4 horas.",
                    Type = "Warning",
                    RouteId = execution.RouteId,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
        await _alertRepository.SaveChangesAsync();
    }
}
