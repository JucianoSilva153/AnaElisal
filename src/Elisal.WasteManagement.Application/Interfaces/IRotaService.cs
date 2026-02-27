using Elisal.WasteManagement.Application.DTOs;
using Elisal.WasteManagement.Domain.Entities;

namespace Elisal.WasteManagement.Application.Interfaces;

public interface IRotaService
{
    Task<List<CollectionPointDto>> OtimizarRotaAsync(List<int> pontoIds);
    Task<double> CalcularDistanciaTotal(List<CollectionPointDto> pontos);
    Task<double> CalcularDistanciaTotal(List<CollectionPoint> pontos);
}
