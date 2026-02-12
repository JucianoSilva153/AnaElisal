using Elisal.WasteManagement.Application.DTOs;

namespace Elisal.WasteManagement.Application.Interfaces;

public interface IRotaService
{
    Task<List<CollectionPointDto>> OtimizarRotaAsync(List<int> pontoIds);
    Task<double> CalcularDistanciaTotal(List<CollectionPointDto> pontos);
}
