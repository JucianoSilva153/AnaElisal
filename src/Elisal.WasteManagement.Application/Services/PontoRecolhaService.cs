using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elisal.WasteManagement.Application.DTOs;
using Elisal.WasteManagement.Application.Interfaces;
using Elisal.WasteManagement.Domain.Entities;
using Elisal.WasteManagement.Domain.Interfaces;

namespace Elisal.WasteManagement.Application.Services;

public class PontoRecolhaService : IPontoRecolhaService
{
    private readonly ICollectionPointRepository _collectionPointRepository;

    public PontoRecolhaService(ICollectionPointRepository collectionPointRepository)
    {
        _collectionPointRepository = collectionPointRepository;
    }

    public async Task<CollectionPointDto> CriarPontoAsync(CreateCollectionPointDto dto)
    {
        // Simple validation
        if (string.IsNullOrWhiteSpace(dto.Nome))
            throw new ArgumentException("Nome do ponto é obrigatório.");

        var pontoRecolha = new CollectionPoint
        {
            Name = dto.Nome,
            Address = dto.Endereco,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            Capacity = dto.Capacidade,
            Municipality = dto.Municipio,
            IsActive = true
        };

        await _collectionPointRepository.AddAsync(pontoRecolha);
        await _collectionPointRepository.SaveChangesAsync();
        return pontoRecolha.ToDto();
    }

    public async Task AtualizarCapacidadeAsync(int pontoId, double novaCapacidade)
    {
        var ponto = await _collectionPointRepository.GetByIdAsync(pontoId);
        if (ponto == null)
            throw new InvalidOperationException("Ponto de recolha não encontrado.");

        ponto.Capacity = novaCapacidade;
        _collectionPointRepository.Update(ponto);
        await _collectionPointRepository.SaveChangesAsync();
    }

    public async Task<IEnumerable<CollectionPointDto>> ObterPontosProximosAsync(double latitude, double longitude, double raioKm)
    {
        var allPoints = await _collectionPointRepository.GetAllAsync();
        
        // In-memory filtering for demo (not efficient for huge datasets, but spatial DB requires more setup)
        return allPoints
            .Where(p => CalculateDistance(latitude, longitude, p.Latitude, p.Longitude) <= raioKm)
            .Select(p => p.ToDto());
    }

    public async Task<double> ObterOcupacaoAtualAsync(int pontoId)
    {
        // Simulating occupation logic. Real logging would require IoT sensors or checking recent logs.
        // For now, return a random simulated value or based on recent collection frequency simulation.
        var ponto = await _collectionPointRepository.GetByIdAsync(pontoId);
        if (ponto == null) return 0;

        // Simulated logic: purely random for demo purposes as requested structure implies simulation/estimation
        return new Random().NextDouble() * 100; // 0 to 100%
    }

    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        // Haversine formula
        var R = 6371; // Radius of Earth in km
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private double ToRadians(double angle)
    {
        return Math.PI * angle / 180.0;
    }
}
