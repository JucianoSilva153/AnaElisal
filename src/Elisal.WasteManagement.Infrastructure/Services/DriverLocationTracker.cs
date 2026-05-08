using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Elisal.WasteManagement.Application.Interfaces;

namespace Elisal.WasteManagement.Infrastructure.Services;

/// <summary>
/// Singleton em memória que mantém a última posição GPS conhecida
/// de cada motorista com uma execução de rota activa.
/// Posições com mais de 10 minutos sem actualização são ignoradas.
/// </summary>
public class DriverLocationTracker : IDriverLocationTracker
{
    private static readonly TimeSpan _staleness = TimeSpan.FromMinutes(10);
    private readonly ConcurrentDictionary<int, ActiveDriverPosition> _positions = new();

    public void UpdatePosition(int driverId, int executionId, double lat, double lon)
    {
        _positions[driverId] = new ActiveDriverPosition(driverId, executionId, lat, lon, DateTime.UtcNow);
    }

    public void RemoveDriver(int driverId)
    {
        _positions.TryRemove(driverId, out _);
    }

    public IReadOnlyList<ActiveDriverPosition> GetAll()
    {
        var cutoff = DateTime.UtcNow - _staleness;
        return _positions.Values
            .Where(p => p.LastSeen >= cutoff)
            .ToList();
    }
}
