using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Elisal.WasteManagement.Application.Interfaces;
using Elisal.WasteManagement.Domain.Entities;
using Elisal.WasteManagement.Domain.Interfaces;

namespace Elisal.WasteManagement.Application.Services;

/// <summary>
/// Detecta conflitos de recolha em tempo real:
/// quando dois motoristas em execuções activas com um ponto em comum
/// estão ambos a menos de 50 metros desse ponto, o ponto é
/// automaticamente marcado como inacessível para o segundo motorista
/// (o que enviou a posição que despoletou a detecção).
/// </summary>
public class RouteConflictService : IRouteConflictService
{
    private const double ConflictRadiusMeters = 50.0;

    private readonly IDriverLocationTracker _tracker;
    private readonly IRoutePointExecutionStatusRepository _statusRepository;
    private readonly IOperationalAlertRepository _alertRepository;
    private readonly IRepository<CollectionPoint> _pointRepository;

    public RouteConflictService(
        IDriverLocationTracker tracker,
        IRoutePointExecutionStatusRepository statusRepository,
        IOperationalAlertRepository alertRepository,
        IRepository<CollectionPoint> pointRepository)
    {
        _tracker = tracker;
        _statusRepository = statusRepository;
        _alertRepository = alertRepository;
        _pointRepository = pointRepository;
    }

    public async Task<PointConflictResult?> CheckAndResolveConflictAsync(
        int driverId, int executionId, double driverLat, double driverLon)
    {
        // 1. Obter pontos pendentes (não concluídos, não pulados) da execução actual
        var pendingPoints = await _statusRepository.GetPendingByExecutionIdAsync(executionId);

        if (!pendingPoints.Any()) return null;

        // 2. Encontrar o ponto pendente mais próximo do motorista dentro do raio
        RoutePointExecutionStatus? nearestStatus = null;
        double nearestDistance = double.MaxValue;

        foreach (var ps in pendingPoints)
        {
            var dist = HaversineMeters(
                driverLat, driverLon,
                ps.CollectionPoint.Latitude,
                ps.CollectionPoint.Longitude);

            if (dist <= ConflictRadiusMeters && dist < nearestDistance)
            {
                nearestDistance = dist;
                nearestStatus = ps;
            }
        }

        if (nearestStatus == null) return null;

        var targetPoint = nearestStatus.CollectionPoint;

        // 3. Verificar se outro motorista activo também está perto deste ponto
        var otherDrivers = _tracker.GetAll()
            .Where(p => p.DriverId != driverId)
            .ToList();

        int conflictingDriverId = 0;
        foreach (var other in otherDrivers)
        {
            var otherDist = HaversineMeters(
                other.Lat, other.Lon,
                targetPoint.Latitude,
                targetPoint.Longitude);

            if (otherDist <= ConflictRadiusMeters)
            {
                // Verificar se a rota do outro motorista também inclui este ponto
                var otherHasPoint = await _statusRepository.HasAnyPendingAtPointAsync(other.ExecutionId, targetPoint.Id);

                if (otherHasPoint)
                {
                    conflictingDriverId = other.DriverId;
                    break;
                }
            }
        }

        if (conflictingDriverId == 0) return null;

        // 4. Conflito confirmado — marcar ponto como inacessível para este motorista
        nearestStatus.IsSkipped = true;
        nearestStatus.SkipReason = "Conflito de recolha: outro motorista está neste ponto.";
        await _statusRepository.UpdateAsync(nearestStatus);

        // 5. Criar alerta operacional (evitar duplicados não lidos)
        var allAlerts = await _alertRepository.GetAllAsync();
        var hasAlert = allAlerts.Any(a =>
            a.CollectionPointId == targetPoint.Id &&
            !a.IsRead &&
            a.Type == "Warning" &&
            a.Title == "Conflito de Ponto de Recolha");

        if (!hasAlert)
        {
            await _alertRepository.AddAsync(new OperationalAlert
            {
                Title = "Conflito de Ponto de Recolha",
                Message = $"Dois motoristas estiveram simultaneamente no ponto '{targetPoint.Name}'. " +
                          $"O ponto foi marcado como inacessível automaticamente.",
                Type = "Warning",
                CollectionPointId = targetPoint.Id,
                CreatedAt = DateTime.UtcNow
            });
            await _alertRepository.SaveChangesAsync();
        }

        return new PointConflictResult(
            targetPoint.Id,
            targetPoint.Name,
            conflictingDriverId,
            executionId);
    }

    /// <summary>
    /// Fórmula de Haversine — distância em metros entre dois pontos GPS.
    /// </summary>
    private static double HaversineMeters(
        double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000;
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(lat1 * Math.PI / 180)
              * Math.Cos(lat2 * Math.PI / 180)
              * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }
}
