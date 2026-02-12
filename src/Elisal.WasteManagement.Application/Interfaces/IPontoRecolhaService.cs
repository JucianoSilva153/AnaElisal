using Elisal.WasteManagement.Application.DTOs;

namespace Elisal.WasteManagement.Application.Interfaces;

public interface IPontoRecolhaService
{
    Task<CollectionPointDto> CriarPontoAsync(CreateCollectionPointDto dto);
    Task AtualizarCapacidadeAsync(int pontoId, double novaCapacidade);
    Task<IEnumerable<CollectionPointDto>> ObterPontosProximosAsync(double latitude, double longitude, double raioKm);
    Task<double> ObterOcupacaoAtualAsync(int pontoId); // Simulação ou cálculo real
}
