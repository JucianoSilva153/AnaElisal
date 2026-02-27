using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elisal.WasteManagement.Application.DTOs;
using Elisal.WasteManagement.Application.Interfaces;
using Elisal.WasteManagement.Domain.Entities;
using Elisal.WasteManagement.Domain.Interfaces;

namespace Elisal.WasteManagement.Application.Services;

public class RotaService : IRotaService
{
    private readonly IRepository<CollectionPoint> _pointRepo;

    public RotaService(IRepository<CollectionPoint> pointRepo)
    {
        _pointRepo = pointRepo;
    }

    public async Task<List<CollectionPointDto>> OtimizarRotaAsync(List<int> pontoIds)
    {
        var allPoints = await _pointRepo.GetAllAsync();
        var selectedPoints = allPoints.Where(p => pontoIds.Contains(p.Id)).ToList();

        if (selectedPoints.Count <= 1)
            return selectedPoints.Select(p => p.ToDto()).ToList();

        // Nearest Neighbor Algorithm
        var optimized = new List<CollectionPoint>();
        var remaining = new List<CollectionPoint>(selectedPoints);
        var current = remaining.First(); // Start from first point
        remaining.Remove(current);
        optimized.Add(current);

        while (remaining.Any())
        {
            var nearest = remaining
                .OrderBy(p => CalcularDistancia(current.Latitude, current.Longitude, p.Latitude, p.Longitude))
                .First();

            optimized.Add(nearest);
            remaining.Remove(nearest);
            current = nearest;
        }

        return optimized.Select(p => p.ToDto()).ToList();
    }

    public async Task<double> CalcularDistanciaTotal(List<CollectionPointDto> pontos)
    {
        if (pontos == null || pontos.Count <= 1)
            return 0;

        double totalDistance = 0;
        for (int i = 0; i < pontos.Count - 1; i++)
        {
            totalDistance += CalcularDistancia(
                pontos[i].Latitude,
                pontos[i].Longitude,
                pontos[i + 1].Latitude,
                pontos[i + 1].Longitude);
        }

        return totalDistance;
    }

    public async Task<double> CalcularDistanciaTotal(List<CollectionPoint> pontos)
    {
        if (pontos == null || pontos.Count <= 1)
            return 0;

        double totalDistance = 0;
        for (int i = 0; i < pontos.Count - 1; i++)
        {
            totalDistance += CalcularDistancia(
                pontos[i].Latitude,
                pontos[i].Longitude,
                pontos[i + 1].Latitude,
                pontos[i + 1].Longitude);
        }

        return totalDistance;
    }

    // Haversine formula for distance calculation
    private double CalcularDistancia(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Earth radius in km
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }
}
